
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using RaceData222.DTOs;
using RaceDay222.DatabaseContext;

namespace RaceDay222.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly RaceDayContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<string> _hasher = new();

        public AuthController(
            RaceDayContext context,
            IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // POST: api/auth/register/participant
        [HttpPost("register/participant")]
        public async Task<IActionResult> RegisterParticipant(
            [FromBody] RaceData222.DTOs.LoginRequest request)
        {
            return await Register(request, "Participant");
        }

        // POST: api/auth/register/organiser
        [HttpPost("register/organiser")]
        public async Task<IActionResult> RegisterOrganiser(
            [FromBody] RaceData222.DTOs.LoginRequest request)
        {
            return await Register(request, "Organiser");
        }

        private async Task<IActionResult> Register(
            RaceData222.DTOs.LoginRequest request, string role)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            if (request.Password.Length < 8)
                return BadRequest(new
                {
                    message = "Password must be at least 8 characters."
                });

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            // Emails must be unique across both account tables.
            const string checkSql = @"
                SELECT
                    (SELECT COUNT(*) FROM ORGANISER WHERE Email = @Email)
                  + (SELECT COUNT(*) FROM PARTICIPANT WHERE Email = @Email)";

            using var check = new SqlCommand(checkSql, connection);
            check.Parameters.AddWithValue("@Email", request.Email.Trim());

            int exists = Convert.ToInt32(await check.ExecuteScalarAsync());

            if (exists > 0)
                return Conflict(new
                {
                    message = "An account with this email already exists."
                });

            string table = role == "Organiser"
                ? "ORGANISER"
                : "PARTICIPANT";

            string hash = _hasher.HashPassword(
                request.Email.Trim().ToLowerInvariant(),
                request.Password);

            string sql = $@"
                INSERT INTO {table}
                    (FirstName, LastName, Email, Password)
                VALUES
                    (@FirstName, @LastName, @Email, @Password)";

            // This simple registration endpoint currently expects
            // only email/password. Name fields are added below in Step 2.
            return BadRequest(new
            {
                message = "Use the full registration model in Step 2."
            });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(
            [FromBody] RaceData222.DTOs.LoginRequest request)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            // Search the organiser table first.
            var account = await FindAccount(
                connection, "ORGANISER",
                "OrganiserID", request.Email);

            string role = "Organiser";

            if (account == null)
            {
                account = await FindAccount(
                    connection, "PARTICIPANT",
                    "ParticipantID", request.Email);

                role = "Participant";
            }

            if (account == null)
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });

            var verification = _hasher.VerifyHashedPassword(
                request.Email.Trim().ToLowerInvariant(),
                account.Value.PasswordHash,
                request.Password);

            if (verification == PasswordVerificationResult.Failed)
                return Unauthorized(new
                {
                    message = "Invalid email or password."
                });

            string token = CreateToken(
                account.Value.Id, account.Value.Name, role);

            return Ok(new LoginResponse
            {
                Token = token,
                Role = role,
                UserId = account.Value.Id,
                Name = account.Value.Name
            });
        }

        private static async Task<(int Id, string Name, string PasswordHash)?>
            FindAccount(
                SqlConnection connection,
                string table,
                string idColumn,
                string email)
        {
            // Table and ID names are fixed internally, never supplied
            // by the API request.
            string sql = $@"
                SELECT {idColumn}, FirstName, LastName, Password
                FROM {table}
                WHERE Email = @Email";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email.Trim());

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return (
                Convert.ToInt32(reader[idColumn]),
                reader["FirstName"].ToString() + " " +
                    reader["LastName"].ToString(),
                reader["Password"].ToString() ?? ""
            );
        }

        private string CreateToken(int userId, string name, string role)
        {
            string key = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "JWT signing key is missing.");

            var claims = new List<Claim>
{
    new Claim(
        ClaimTypes.NameIdentifier,
        userId.ToString()),

    new Claim(
        ClaimTypes.Name,
        name),

    new Claim(
        ClaimTypes.Role,
        role)
};

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key));

            var credentials = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}