using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay222.DatabaseContext;

namespace RaceData222.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EnrollmentsController(IConfiguration configuration)
        {
            _context = new RaceDayContext(configuration);
        }

        // GET: api/Enrollments
        // Organisers can view all enrolments.
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetAllEnrollments()
        {
            var enrollments = new List<object>();

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT
                    en.EnrollmentID,
                    en.ParticipantID,
                    p.FirstName,
                    p.LastName,
                    en.EventID,
                    e.EventName,
                    en.EnrollmentDate,
                    en.Status
                FROM Enrollment en
                INNER JOIN Participant p
                    ON en.ParticipantID = p.ParticipantID
                INNER JOIN [Event] e
                    ON en.EventID = e.EventID
                ORDER BY en.EnrollmentDate DESC";

            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                enrollments.Add(new
                {
                    EnrollmentID = reader["EnrollmentID"],
                    ParticipantID = reader["ParticipantID"],
                    ParticipantName =
                        reader["FirstName"] + " " + reader["LastName"],
                    EventID = reader["EventID"],
                    EventName = reader["EventName"],
                    EnrollmentDate = reader["EnrollmentDate"],
                    Status = reader["Status"]
                });
            }

            return Ok(enrollments);
        }

        // GET: api/Enrollments/my-enrollments
        // Participants can view their own enrolments.
        [HttpGet("my-enrollments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyEnrollments()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int participantId))
            {
                return Unauthorized("Invalid participant identity.");
            }

            var enrollments = new List<object>();

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT
                    en.EnrollmentID,
                    en.EventID,
                    e.EventName,
                    en.EnrollmentDate,
                    en.Status
                FROM Enrollment en
                INNER JOIN [Event] e
                    ON en.EventID = e.EventID
                WHERE en.ParticipantID = @ParticipantID
                ORDER BY en.EnrollmentDate DESC";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@ParticipantID", participantId);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                enrollments.Add(new
                {
                    EnrollmentID = reader["EnrollmentID"],
                    EventID = reader["EventID"],
                    EventName = reader["EventName"],
                    EnrollmentDate = reader["EnrollmentDate"],
                    Status = reader["Status"]
                });
            }

            return Ok(enrollments);
        }

        // POST: api/Enrollments/5
        // Enrol the logged-in participant in event 5.
        [HttpPost("{eventId:int}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> EnrollInEvent(int eventId)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int participantId))
            {
                return Unauthorized("Invalid participant identity.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            // Check that the event exists.
            string eventSql = @"
                SELECT COUNT(*)
                FROM [Event]
                WHERE EventID = @EventID";

            using var eventCommand =
                new SqlCommand(eventSql, connection);

            eventCommand.Parameters.AddWithValue("@EventID", eventId);

            int eventExists = Convert.ToInt32(
                await eventCommand.ExecuteScalarAsync());

            if (eventExists == 0)
            {
                return NotFound("Event not found.");
            }

            // Prevent duplicate enrolments.
            string duplicateSql = @"
                SELECT COUNT(*)
                FROM Enrollment
                WHERE ParticipantID = @ParticipantID
                  AND EventID = @EventID";

            using var duplicateCommand =
                new SqlCommand(duplicateSql, connection);

            duplicateCommand.Parameters.AddWithValue(
                "@ParticipantID", participantId);

            duplicateCommand.Parameters.AddWithValue(
                "@EventID", eventId);

            int existingEnrollment = Convert.ToInt32(
                await duplicateCommand.ExecuteScalarAsync());

            if (existingEnrollment > 0)
            {
                return Conflict(
                    "You are already enrolled in this event.");
            }

            // Create the enrolment.
            string insertSql = @"
                INSERT INTO Enrollment
                    (ParticipantID, EventID, EnrollmentDate, Status)
                VALUES
                    (@ParticipantID, @EventID, @EnrollmentDate, @Status)";

            using var insertCommand =
                new SqlCommand(insertSql, connection);

            insertCommand.Parameters.AddWithValue(
                "@ParticipantID", participantId);

            insertCommand.Parameters.AddWithValue(
                "@EventID", eventId);

            insertCommand.Parameters.AddWithValue(
                "@EnrollmentDate", DateTime.Now);

            insertCommand.Parameters.AddWithValue(
                "@Status", "Enrolled");

            await insertCommand.ExecuteNonQueryAsync();

            return Ok(new
            {
                message = "Successfully enrolled in the event.",
                eventId = eventId
            });
        }

        // DELETE: api/Enrollments/5
        // A participant can cancel their own enrolment.
        [HttpDelete("{enrollmentId:int}")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> CancelEnrollment(
            int enrollmentId)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int participantId))
            {
                return Unauthorized("Invalid participant identity.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                DELETE FROM Enrollment
                WHERE EnrollmentID = @EnrollmentID
                  AND ParticipantID = @ParticipantID";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@EnrollmentID", enrollmentId);

            command.Parameters.AddWithValue(
                "@ParticipantID", participantId);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound(
                    "Enrolment not found or you do not own this enrolment.");
            }

            return Ok(new
            {
                message = "Enrolment cancelled successfully."
            });
        }
    }
}