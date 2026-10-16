using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay222.DatabaseContext;

namespace RaceDay222.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Participant")]
    public class ParticipantProfilesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public ParticipantProfilesController(
            IConfiguration configuration)
        {
            _context = new RaceDayContext(configuration);
        }

        // GET: api/ParticipantProfiles/me
        // Retrieve the logged-in participant's profile.
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
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
                SELECT ParticipantID, FirstName, LastName,
                       Email, PhoneNumber
                FROM Participant_Profile
                WHERE ParticipantID = @ParticipantID";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@ParticipantID", participantId);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound("Participant profile not found.");
            }

            var profile = new
            {
                ParticipantID = reader["ParticipantID"],
                FirstName = reader["FirstName"],
                LastName = reader["LastName"],
                Email = reader["Email"],
                PhoneNumber = reader["PhoneNumber"]
            };

            return Ok(profile);
        }

        // PUT: api/ParticipantProfiles/me
        // Update the logged-in participant's profile.
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateParticipantProfileRequest request)
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

            string sql = @"
                UPDATE Participant_Profile
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
                return NotFound(
                    "Participant profile not found.");
            }

            return Ok(new
            {
                message = "Participant profile updated successfully."
            });
        }
    }

    public class UpdateParticipantProfileRequest
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
    }
}