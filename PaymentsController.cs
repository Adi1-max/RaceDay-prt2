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
    public class PaymentsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public PaymentsController(IConfiguration configuration)
        {
            _context = new RaceDayContext(configuration);
        }

        // GET: api/Payments
        // Organisers can view all payments.
        [HttpGet]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = new List<object>();

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT
                    p.PaymentID,
                    p.EnrollmentID,
                    e.ParticipantID,
                    e.EventID,
                    p.Amount,
                    p.PaymentDate,
                    p.Status
                FROM Payment p
                INNER JOIN Enrollment e
                    ON p.EnrollmentID = e.EnrollmentID
                ORDER BY p.PaymentDate DESC";

            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                payments.Add(new
                {
                    PaymentID = reader["PaymentID"],
                    EnrollmentID = reader["EnrollmentID"],
                    ParticipantID = reader["ParticipantID"],
                    EventID = reader["EventID"],
                    Amount = reader["Amount"],
                    PaymentDate = reader["PaymentDate"],
                    Status = reader["Status"]
                });
            }

            return Ok(payments);
        }

        // GET: api/Payments/my-payments
        // Participants can view their own payments.
        [HttpGet("my-payments")]
        [Authorize(Roles = "Participant")]
        public async Task<IActionResult> GetMyPayments()
        {
            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int participantId))
            {
                return Unauthorized("Invalid participant identity.");
            }

            var payments = new List<object>();

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT
                    p.PaymentID,
                    p.EnrollmentID,
                    e.EventID,
                    ev.EventName,
                    p.Amount,
                    p.PaymentDate,
                    p.Status
                FROM Payment p
                INNER JOIN Enrollment e
                    ON p.EnrollmentID = e.EnrollmentID
                INNER JOIN [Event] ev
                    ON e.EventID = ev.EventID
                WHERE e.ParticipantID = @ParticipantID
                ORDER BY p.PaymentDate DESC";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@ParticipantID", participantId);

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                payments.Add(new
                {
                    PaymentID = reader["PaymentID"],
                    EnrollmentID = reader["EnrollmentID"],
                    EventID = reader["EventID"],
                    EventName = reader["EventName"],
                    Amount = reader["Amount"],
                    PaymentDate = reader["PaymentDate"],
                    Status = reader["Status"]
                });
            }

            return Ok(payments);
        }

        // GET: api/Payments/5
        // Participants can view a payment belonging to them.
        // Organisers can view any payment.
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Participant,Organiser")]
        public async Task<IActionResult> GetPayment(int id)
        {
            bool isOrganiser = User.IsInRole("Organiser");

            if (!int.TryParse(
                User.FindFirstValue(ClaimTypes.NameIdentifier),
                out int currentUserId))
            {
                return Unauthorized("Invalid user identity.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT
                    p.PaymentID,
                    p.EnrollmentID,
                    e.ParticipantID,
                    e.EventID,
                    ev.EventName,
                    p.Amount,
                    p.PaymentDate,
                    p.Status
                FROM Payment p
                INNER JOIN Enrollment e
                    ON p.EnrollmentID = e.EnrollmentID
                INNER JOIN [Event] ev
                    ON e.EventID = ev.EventID
                WHERE p.PaymentID = @PaymentID";

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@PaymentID", id);

            using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound("Payment not found.");
            }

            int paymentParticipantId =
                Convert.ToInt32(reader["ParticipantID"]);

            if (!isOrganiser && paymentParticipantId != currentUserId)
            {
                return Forbid();
            }

            var payment = new
            {
                PaymentID = reader["PaymentID"],
                EnrollmentID = reader["EnrollmentID"],
                ParticipantID = reader["ParticipantID"],
                EventID = reader["EventID"],
                EventName = reader["EventName"],
                Amount = reader["Amount"],
                PaymentDate = reader["PaymentDate"],
                Status = reader["Status"]
            };

            return Ok(payment);
        }
    }
}