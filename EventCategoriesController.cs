
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay222.DatabaseContext;

namespace RaceDay222.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EventCategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;
    public EventCategoriesController(IConfiguration configuration)
    {
            using var connection = _context.GetConnection();
        }
        // GET: api/EventCategories
        // View all event-category assignments.
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var assignments = new List<object>();

            
            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    ec.EventID,
                    e.EventName,
                    ec.CategoryID,
                    c.CategoryName,
                    c.Description,
                    c.Type
                FROM [Event_Category] ec
                INNER JOIN [Event] e ON ec.EventID = e.EventID
                INNER JOIN Category c ON ec.CategoryID = c.CategoryID
                ORDER BY ec.EventID, ec.CategoryID";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                assignments.Add(new
                {
                    EventID = reader.GetInt32(0),
                    EventName = reader.GetString(1),
                    CategoryID = reader.GetInt32(2),
                    CategoryName = reader.GetString(3),
                    Description = reader.IsDBNull(4)
                        ? null : reader.GetString(4),
                    Type = reader.IsDBNull(5)
                        ? null : reader.GetString(5)
                });
            }

            return Ok(assignments);
        }

        // GET: api/EventCategories/event/5
        // View categories assigned to a specific event.
        [HttpGet("event/{eventId:int}")]
        public async Task<IActionResult> GetByEvent(int eventId)
        {
            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string query = @"
                SELECT
                    ec.EventID,
                    e.EventName,
                    ec.CategoryID,
                    c.CategoryName,
                    c.Description,
                    c.Type
                FROM [Event_Category] ec
                INNER JOIN [Event] e ON ec.EventID = e.EventID
                INNER JOIN Category c ON ec.CategoryID = c.CategoryID
                WHERE ec.EventID = @EventID
                ORDER BY ec.CategoryID";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@EventID", eventId);

            using var reader = await command.ExecuteReaderAsync();
            var assignments = new List<object>();

            while (await reader.ReadAsync())
            {
                assignments.Add(new
                {
                    EventID = reader.GetInt32(0),
                    EventName = reader.GetString(1),
                    CategoryID = reader.GetInt32(2),
                    CategoryName = reader.GetString(3),
                    Description = reader.IsDBNull(4)
                        ? null : reader.GetString(4),
                    Type = reader.IsDBNull(5)
                        ? null : reader.GetString(5)
                });
            }

            return Ok(assignments);
        }

        // POST: api/EventCategories
        // Assign a category to an event.
        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> AssignCategory(
            [FromBody] EventCategoryRequest request)
        {
            if (request.EventID <= 0 || request.CategoryID <= 0)
            {
                return BadRequest("Valid EventID and CategoryID are required.");
            }

            var organiserId = GetOrganiserId();

            if (organiserId == null)
            {
                return Unauthorized("Organiser ID was not found in the token.");
            }

            using var connection = _context.GetConnection();
            
            await connection.OpenAsync();

            // Verify that the event belongs to this organiser.
            string ownerQuery = @"
                SELECT COUNT(*)
                FROM [Event]
                WHERE EventID = @EventID
                  AND OrganiserID = @OrganiserID";

            using (var ownerCommand = new SqlCommand(ownerQuery, connection))
            {
                ownerCommand.Parameters.AddWithValue(
                    "@EventID", request.EventID);
                ownerCommand.Parameters.AddWithValue(
                    "@OrganiserID", organiserId.Value);

                int ownsEvent = Convert.ToInt32(
                    await ownerCommand.ExecuteScalarAsync());

                if (ownsEvent == 0)
                {
                    return NotFound(
                        "Event not found or you do not own this event.");
                }
            }

            // Check whether the category exists.
            string categoryQuery = @"
                SELECT COUNT(*)
                FROM Category
                WHERE CategoryID = @CategoryID";

            using (var categoryCommand =
                   new SqlCommand(categoryQuery, connection))
            {
                categoryCommand.Parameters.AddWithValue(
                    "@CategoryID", request.CategoryID);

                int categoryExists = Convert.ToInt32(
                    await categoryCommand.ExecuteScalarAsync());

                if (categoryExists == 0)
                {
                    return NotFound("Category not found.");
                }
            }

            // Prevent duplicate assignments.
            string duplicateQuery = @"
                SELECT COUNT(*)
                FROM [Event_Category]
                WHERE EventID = @EventID
                  AND CategoryID = @CategoryID";

            using (var duplicateCommand =
                   new SqlCommand(duplicateQuery, connection))
            {
                duplicateCommand.Parameters.AddWithValue(
                    "@EventID", request.EventID);
                duplicateCommand.Parameters.AddWithValue(
                    "@CategoryID", request.CategoryID);

                int alreadyAssigned = Convert.ToInt32(
                    await duplicateCommand.ExecuteScalarAsync());

                if (alreadyAssigned > 0)
                {
                    return Conflict(
                        "This category is already assigned to the event.");
                }
            }

            string insertQuery = @"
                INSERT INTO [Event_Category] (EventID, CategoryID)
                VALUES (@EventID, @CategoryID)";

            using var insertCommand =
                new SqlCommand(insertQuery, connection);

            insertCommand.Parameters.AddWithValue(
                "@EventID", request.EventID);
            insertCommand.Parameters.AddWithValue(
                "@CategoryID", request.CategoryID);

            await insertCommand.ExecuteNonQueryAsync();

            return Ok(new
            {
                Message = "Category assigned to event successfully.",
                request.EventID,
                request.CategoryID
            });
        }

        // DELETE: api/EventCategories/5/2
        // Remove a category from an event.
        [HttpDelete("{eventId:int}/{categoryId:int}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> RemoveCategory(
            int eventId, int categoryId)
        {
            var organiserId = GetOrganiserId();

            if (organiserId == null)
            {
                return Unauthorized("Organiser ID was not found in the token.");
            }

            using var connection = _context.GetConnection();
            await connection.OpenAsync();

            string deleteQuery = @"
                DELETE FROM [Event_Category]
                WHERE EventID = @EventID
                  AND CategoryID = @CategoryID
                  AND EXISTS (
                      SELECT 1
                      FROM [Event] e
                      WHERE e.EventID = [Event_Category].EventID
                        AND e.OrganiserID = @OrganiserID
                  )";

            using var command = new SqlCommand(deleteQuery, connection);

            command.Parameters.AddWithValue("@EventID", eventId);
            command.Parameters.AddWithValue("@CategoryID", categoryId);
            command.Parameters.AddWithValue(
                "@OrganiserID", organiserId.Value);

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound(
                    "Assignment not found, or you do not own this event.");
            }

            return Ok(new
            {
                Message = "Category removed from event successfully.",
                EventID = eventId,
                CategoryID = categoryId
            });
        }

        // Reads the organiser ID from the authenticated JWT.
        private int? GetOrganiserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(idClaim, out int organiserId))
            {
                return organiserId;
            }

            return null;
        }
    }

    // Request body for assigning a category to an event.
    public class EventCategoryRequest
    {
        public int EventID { get; set; }
        public int CategoryID { get; set; }
    }
}