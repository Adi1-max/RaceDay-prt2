using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using Microsoft.AspNetCore.Mvc;
using RaceDay222.DatabaseContext;

namespace RaceDay222.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public TestController(RaceDayContext context)
        {
            _context = context;
        }

        [HttpGet("database")]
        public async Task<IActionResult> TestDatabase()
        {
            try
            {
                using var connection = _context.GetConnection();
                await connection.OpenAsync();

                return Ok(new
                {
                    message = "Database connection successful!",
                    database = connection.Database
                });
            }
            catch (Exception)
            {
                // Log the exception in production rather than returning
                // internal connection details to API clients.
                return StatusCode(500, new
                {
                    message = "Database connection failed."
                });
            }
        }
    }
}
