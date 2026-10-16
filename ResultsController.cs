
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay222.DatabaseContext;
using RaceDay222.DatabaseContext;

namespace RaceDay222.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ResultsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ResultsController(IConfiguration configuration)
        {
            _context = new RaceDayContext(configuration);
        }

        // GET: api/Results
        // Organisers can view all results.
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetAllResults()
        {
            var results = new List<object>();

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT
                    r.ResultID,
                    r.EnrollmentID,
                    e.EventID,
                    ev.EventName,
                    e.ParticipantID,
                    p.FirstName,
                    p.LastName,
                    r.Position,
                    r.FinishTime,
                    r.RecordedAt
                FROM Result r
                INNER JOIN Enrollment e
                    ON r.EnrollmentID = e.EnrollmentID
                INNER JOIN [Event] ev
                    ON e.EventID = ev.EventID
                INNER JOIN Participant p
                    ON e.ParticipantID = p.ParticipantID
                ORDER BY ev.EventName, r.Position";

            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    ResultID = reader["ResultID"],
                    EnrollmentID = reader["EnrollmentID"],
                    EventID = reader["EventID"],
                    EventName = reader["EventName"],
                    ParticipantID = reader["ParticipantID"],
                    ParticipantName =
                        reader["FirstName"] + " " + reader["LastName"],
                    Position = reader["Position"],
                    FinishTime = reader["FinishTime"],
                    RecordedAt = reader["RecordedAt"]
                });
            }

            return Ok(results);
        }

        // GET: api/Results/my-results
        // Participants can view their own results.
        [HttpGet("my-results")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyResults()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int participantId))
            {
                return Unauthorized("Invalid participant identity.");
            }

            var results = new List<object>();

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT
                    r.ResultID,
                    e.EventID,
                    ev.EventName,
                    r.Position,
                    r.FinishTime,
                    r.RecordedAt
                FROM Result r
                INNER JOIN Enrollment e
                    ON r.EnrollmentID = e.EnrollmentID
                INNER JOIN [Event] ev
                    ON e.EventID = ev.EventID
                WHERE e.ParticipantID = @ParticipantID
                ORDER BY r.RecordedAt DESC";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@ParticipantID", participantId);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new
                {
                    ResultID = reader["ResultID"],
                    EventID = reader["EventID"],
                    EventName = reader["EventName"],
                    Position = reader["Position"],
                    FinishTime = reader["FinishTime"],
                    RecordedAt = reader["RecordedAt"]
                });
            }

            return Ok(results);
        }

        // POST: api/Results
        // Organisers record a result for an enrolment.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateResult(
            [FromBody] CreateResultRequest request)
        {
            if (request.EnrollmentID <= 0 ||
                request.Position <= 0 ||
                request.FinishTime <= TimeSpan.Zero)
            {
                return BadRequest(
                    "Enrollment ID, position and a positive finish time are required.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            // Confirm that the enrolment exists.
            string checkSql = @"
                SELECT COUNT(*)
                FROM Enrollment
                WHERE EnrollmentID = @EnrollmentID";

            using var checkCommand =
                new SqlCommand(checkSql, connection);

            checkCommand.Parameters.AddWithValue(
                "@EnrollmentID", request.EnrollmentID);

            int enrollmentExists = Convert.ToInt32(
                await checkCommand.ExecuteScalarAsync());

            if (enrollmentExists == 0)
            {
                return NotFound("Enrolment not found.");
            }

            // Prevent duplicate results for the same enrolment.
            string duplicateSql = @"
                SELECT COUNT(*)
                FROM Result
                WHERE EnrollmentID = @EnrollmentID";

            using var duplicateCommand =
                new SqlCommand(duplicateSql, connection);

            duplicateCommand.Parameters.AddWithValue(
                "@EnrollmentID", request.EnrollmentID);

            int existingResult = Convert.ToInt32(
                await duplicateCommand.ExecuteScalarAsync());

            if (existingResult > 0)
            {
                return Conflict(
                    "A result already exists for this enrolment.");
            }

            string insertSql = @"
                INSERT INTO Result
                    (EnrollmentID, Position, FinishTime, RecordedAt)
                VALUES
                    (@EnrollmentID, @Position, @FinishTime, @RecordedAt)";

            using var insertCommand =
                new SqlCommand(insertSql, connection);

            insertCommand.Parameters.AddWithValue(
                "@EnrollmentID", request.EnrollmentID);

            insertCommand.Parameters.AddWithValue(
                "@Position", request.Position);

            insertCommand.Parameters.AddWithValue(
                "@FinishTime", request.FinishTime);

            insertCommand.Parameters.AddWithValue(
                "@RecordedAt", DateTime.Now);

            await insertCommand.ExecuteNonQueryAsync();

            return Ok(new
            {
                message = "Race result recorded successfully."
            });
        }

        // PUT: api/Results/1
        // Organisers can correct a recorded result.
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateResult(
            int id,
            [FromBody] UpdateResultRequest request)
        {
            if (request.Position <= 0 ||
                request.FinishTime <= TimeSpan.Zero)
            {
                return BadRequest(
                    "Position and a positive finish time are required.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                UPDATE Result
                SET Position = @Position,
                    FinishTime = @FinishTime
                WHERE ResultID = @ResultID";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@Position", request.Position);

            command.Parameters.AddWithValue(
                "@FinishTime", request.FinishTime);

            command.Parameters.AddWithValue("@ResultID", id);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound("Result not found.");
            }

            return Ok(new
            {
                message = "Race result updated successfully."
            });
        }
    }

    public class CreateResultRequest
    {
        public int EnrollmentID { get; set; }
        public int Position { get; set; }
        public TimeSpan FinishTime { get; set; }
    }

    public class UpdateResultRequest
    {
        public int Position { get; set; }
        public TimeSpan FinishTime { get; set; }
    }
}