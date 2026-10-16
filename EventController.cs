
using System.Data;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay222.DatabaseContext;

namespace RaceDay222.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public EventsController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/events
        // Anyone with access to the API can browse events.
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = new List<object>();

            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT EventID, OrganiserID, EventName, Description,
                       EventDate, StartTime, Location, Distance,
                       Status, CreatedAt
                FROM Event
                ORDER BY EventDate;";

            using SqlCommand command = new SqlCommand(sql, connection);
            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                events.Add(new
                {
                    EventID = reader.GetInt32(0),
                    OrganiserID = reader.GetInt32(1),
                    EventName = reader.GetString(2),
                    Description = reader.IsDBNull(3)
                        ? null : reader.GetString(3),
                    EventDate = reader.GetDateTime(4),
                    StartTime = reader.GetTimeSpan(5),
                    Location = reader.GetString(6),
                    Distance = reader.IsDBNull(7)
                        ? null : reader.GetString(7),
                    Status = reader.GetString(8),
                    CreatedAt = reader.GetDateTime(9)
                });
            }

            return Ok(events);
        }

        // GET: api/events/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetEvent(int id)
        {
            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT EventID, OrganiserID, EventName, Description,
                       EventDate, StartTime, Location, Distance,
                       Status, CreatedAt
                FROM Event
                WHERE EventID = @EventID;";

            using SqlCommand command = new SqlCommand(sql, connection);
            command.Parameters.Add("@EventID", SqlDbType.Int).Value = id;

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound(new { message = "Event not found." });
            }

            var eventItem = new
            {
                EventID = reader.GetInt32(0),
                OrganiserID = reader.GetInt32(1),
                EventName = reader.GetString(2),
                Description = reader.IsDBNull(3)
                    ? null : reader.GetString(3),
                EventDate = reader.GetDateTime(4),
                StartTime = reader.GetTimeSpan(5),
                Location = reader.GetString(6),
                Distance = reader.IsDBNull(7)
                    ? null : reader.GetString(7),
                Status = reader.GetString(8),
                CreatedAt = reader.GetDateTime(9)
            };

            return Ok(eventItem);
        }

        // POST: api/events
        // Only an authenticated organiser can create an event.
        [Authorize(Roles = "Organiser")]
        [HttpPost]
        public async Task<IActionResult> CreateEvent(
            [FromBody] EventRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int? organiserId = GetOrganiserId();

            if (organiserId == null)
            {
                return Unauthorized(new
                {
                    message = "The login token does not contain a valid organiser ID."
                });
            }

            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO Event
                    (OrganiserID, EventName, Description, EventDate,
                     StartTime, Location, Distance, Status, CreatedAt)
                OUTPUT INSERTED.EventID
                VALUES
                    (@OrganiserID, @EventName, @Description, @EventDate,
                     @StartTime, @Location, @Distance, @Status, GETDATE());";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@OrganiserID", SqlDbType.Int)
                .Value = organiserId.Value;

            command.Parameters.Add("@EventName", SqlDbType.VarChar, 150)
                .Value = request.EventName;

            command.Parameters.Add("@Description", SqlDbType.VarChar, -1)
                .Value = (object?)request.Description ?? DBNull.Value;

            command.Parameters.Add("@EventDate", SqlDbType.Date)
                .Value = request.EventDate.Date;

            command.Parameters.Add("@StartTime", SqlDbType.Time)
                .Value = request.StartTime;

            command.Parameters.Add("@Location", SqlDbType.VarChar, 150)
                .Value = request.Location;

            command.Parameters.Add("@Distance", SqlDbType.VarChar, 50)
                .Value = (object?)request.Distance ?? DBNull.Value;

            command.Parameters.Add("@Status", SqlDbType.VarChar, 20)
                .Value = string.IsNullOrWhiteSpace(request.Status)
                    ? "Upcoming" : request.Status;

            int newEventId = (int)await command.ExecuteScalarAsync();

            return CreatedAtAction(
                nameof(GetEvent),
                new { id = newEventId },
                new
                {
                    eventID = newEventId,
                    message = "Event created successfully."
                });
        }

        // PUT: api/events/1
        // An organiser can update only their own events.
        [Authorize(Roles = "Organiser")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEvent(
            int id, [FromBody] EventRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            int? organiserId = GetOrganiserId();

            if (organiserId == null)
            {
                return Unauthorized(new
                {
                    message = "The login token does not contain a valid organiser ID."
                });
            }

            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                UPDATE Event
                SET EventName = @EventName,
                    Description = @Description,
                    EventDate = @EventDate,
                    StartTime = @StartTime,
                    Location = @Location,
                    Distance = @Distance,
                    Status = @Status
                WHERE EventID = @EventID
                  AND OrganiserID = @OrganiserID;";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@EventID", SqlDbType.Int).Value = id;
            command.Parameters.Add("@OrganiserID", SqlDbType.Int)
                .Value = organiserId.Value;
            command.Parameters.Add("@EventName", SqlDbType.VarChar, 150)
                .Value = request.EventName;
            command.Parameters.Add("@Description", SqlDbType.VarChar, -1)
                .Value = (object?)request.Description ?? DBNull.Value;
            command.Parameters.Add("@EventDate", SqlDbType.Date)
                .Value = request.EventDate.Date;
            command.Parameters.Add("@StartTime", SqlDbType.Time)
                .Value = request.StartTime;
            command.Parameters.Add("@Location", SqlDbType.VarChar, 150)
                .Value = request.Location;
            command.Parameters.Add("@Distance", SqlDbType.VarChar, 50)
                .Value = (object?)request.Distance ?? DBNull.Value;
            command.Parameters.Add("@Status", SqlDbType.VarChar, 20)
                .Value = string.IsNullOrWhiteSpace(request.Status)
                    ? "Upcoming" : request.Status;

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound(new
                {
                    message = "Event not found, or it does not belong to you."
                });
            }

            return Ok(new { message = "Event updated successfully." });
        }

        // DELETE: api/events/1
        // An organiser can delete only their own events.
        [Authorize(Roles = "Organiser")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            int? organiserId = GetOrganiserId();

            if (organiserId == null)
            {
                return Unauthorized(new
                {
                    message = "The login token does not contain a valid organiser ID."
                });
            }

            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                DELETE FROM Event
                WHERE EventID = @EventID
                  AND OrganiserID = @OrganiserID;";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@EventID", SqlDbType.Int).Value = id;
            command.Parameters.Add("@OrganiserID", SqlDbType.Int)
                .Value = organiserId.Value;

            try
            {
                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return NotFound(new
                    {
                        message = "Event not found, or it does not belong to you."
                    });
                }
            }
            catch (SqlException)
            {
                return Conflict(new
                {
                    message = "This event may have enrolments or related records. Remove or handle those records according to the project rules before deleting the event."
                });
            }

            return Ok(new { message = "Event deleted successfully." });
        }

        private int? GetOrganiserId()
        {
            string? id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(id, out int organiserId))
            {
                return organiserId;
            }

            return null;
        }
    }

    public class EventRequest
    {
        public string EventName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public DateTime EventDate { get; set; }

        public TimeSpan StartTime { get; set; }

        public string Location { get; set; } = string.Empty;

        public string? Distance { get; set; }

        public string? Status { get; set; }
    }
}