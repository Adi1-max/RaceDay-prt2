
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay222.DatabaseContext;

namespace RaceDay222.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParticipantsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ParticipantsController(IConfiguration configuration)
        {
            _context = new RaceDayContext(configuration);
        }

        // GET: api/Participants
        // Only organisers can view the participant list.
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetParticipants()
        {
            var participants = new List<object>();

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            const string sql = @"
                SELECT ParticipantID, FirstName, LastName,
                       Email, PhoneNumber, CreatedAt
                FROM Participant
                ORDER BY ParticipantID";

            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                participants.Add(new
                {
                    ParticipantID = reader["ParticipantID"],
                    FirstName = reader["FirstName"],
                    LastName = reader["LastName"],
                    Email = reader["Email"],
                    PhoneNumber = reader["PhoneNumber"],
                    CreatedAt = reader["CreatedAt"]
                });
            }

            return Ok(participants);
        }

        // GET: api/Participants/me
        // A participant can view their own details.
        [HttpGet("me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyDetails()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int participantId))
            {
                return Unauthorized("Invalid participant identity.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            const string sql = @"
                SELECT ParticipantID, FirstName, LastName,
                       Email, PhoneNumber, CreatedAt
                FROM Participant
                WHERE ParticipantID = @ParticipantID";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue(
                "@ParticipantID", participantId);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound("Participant not found.");
            }

            var participant = new
            {
                ParticipantID = reader["ParticipantID"],
                FirstName = reader["FirstName"],
                LastName = reader["LastName"],
                Email = reader["Email"],
                PhoneNumber = reader["PhoneNumber"],
                CreatedAt = reader["CreatedAt"]
            };

            return Ok(participant);
        }

        // GET: api/Participants/5
        // Organisers can view a participant; participants can
        // view only their own details.
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Participant,Organiser")]
        public async Task<IActionResult> GetParticipant(int id)
        {
            bool isOrganiser = User.IsInRole("Organiser");

            if (!isOrganiser)
            {
                if (!int.TryParse(
                    User.FindFirstValue(ClaimTypes.NameIdentifier),
                    out int currentParticipantId))
                {
                    return Unauthorized("Invalid participant identity.");
                }

                if (currentParticipantId != id)
                {
                    return Forbid();
                }
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            const string sql = @"
                SELECT ParticipantID, FirstName, LastName,
                       Email, PhoneNumber, CreatedAt
                FROM Participant
                WHERE ParticipantID = @ParticipantID";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@ParticipantID", id);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound("Participant not found.");
            }

            var participant = new
            {
                ParticipantID = reader["ParticipantID"],
                FirstName = reader["FirstName"],
                LastName = reader["LastName"],
                Email = reader["Email"],
                PhoneNumber = reader["PhoneNumber"],
                CreatedAt = reader["CreatedAt"]
            };

            return Ok(participant);
        }

        // PUT: api/Participants/me
        // Participants can update their own contact details.
        [HttpPut("me")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> UpdateMyDetails(
            [FromBody] UpdateParticipantRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int participantId))
            {
                return Unauthorized("Invalid participant identity.");
            }

            if (string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(
                    "First name, last name and email are required.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            const string sql = @"
                UPDATE Participant
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    Email = @Email,
                    PhoneNumber = @PhoneNumber
                WHERE ParticipantID = @ParticipantID";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@FirstName", request.FirstName.Trim());
            command.Parameters.AddWithValue(
                "@LastName", request.LastName.Trim());
            command.Parameters.AddWithValue(
                "@Email", request.Email.Trim());
            command.Parameters.AddWithValue(
                "@PhoneNumber",
                string.IsNullOrWhiteSpace(request.PhoneNumber)
                    ? DBNull.Value
                    : request.PhoneNumber.Trim());
            command.Parameters.AddWithValue(
                "@ParticipantID", participantId);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound("Participant not found.");
            }

            return Ok(new
            {
                message = "Participant details updated successfully."
            });
        }
    }

    public class UpdateParticipantRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
    }
}