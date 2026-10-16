using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay222.DatabaseContext;

namespace RaceDay222.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Organiser")]
    public class OrganiserProfilesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public OrganiserProfilesController(IConfiguration configuration)
        {
            _context = new RaceDayContext(configuration);
        }

        // GET: api/OrganiserProfiles/me
        // Get the logged-in organiser's profile.
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int organiserId))
            {
                return Unauthorized("Invalid organiser identity.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT OrganiserID, OrganisationName,
                       ContactNumber, Bio
                FROM Organiser_Profile
                WHERE OrganiserID = @OrganiserID";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@OrganiserID", organiserId);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound("Organiser profile not found.");
            }

            var profile = new
            {
                OrganiserID = reader["OrganiserID"],
                OrganisationName = reader["OrganisationName"],
                ContactNumber = reader["ContactNumber"],
                Bio = reader["Bio"]
            };

            return Ok(profile);
        }

        // PUT: api/OrganiserProfiles/me
        // Update the logged-in organiser's profile.
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile(
            [FromBody] UpdateOrganiserProfileRequest request)
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int organiserId))
            {
                return Unauthorized("Invalid organiser identity.");
            }

            if (string.IsNullOrWhiteSpace(request.OrganisationName))
            {
                return BadRequest(
                    "Organisation name is required.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                UPDATE Organiser_Profile
                SET OrganisationName = @OrganisationName,
                    ContactNumber = @ContactNumber,
                    Bio = @Bio
                WHERE OrganiserID = @OrganiserID";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@OrganisationName",
                request.OrganisationName.Trim());

            command.Parameters.AddWithValue(
                "@ContactNumber",
                string.IsNullOrWhiteSpace(request.ContactNumber)
                    ? DBNull.Value
                    : request.ContactNumber.Trim());

            command.Parameters.AddWithValue(
                "@Bio",
                string.IsNullOrWhiteSpace(request.Bio)
                    ? DBNull.Value
                    : request.Bio.Trim());

            command.Parameters.AddWithValue(
                "@OrganiserID", organiserId);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound(
                    "Organiser profile not found. Create the profile first.");
            }

            return Ok(new
            {
                message = "Organiser profile updated successfully."
            });
        }
    }

    public class UpdateOrganiserProfileRequest
    {
        public string OrganisationName { get; set; } = string.Empty;

        public string? ContactNumber { get; set; }

        public string? Bio { get; set; }
    }
}