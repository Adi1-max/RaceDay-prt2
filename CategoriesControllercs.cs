using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using RaceDay222.DatabaseContext;

namespace RaceDay222.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly RaceDayContext _context;

        public CategoriesController(RaceDayContext context)
        {
            _context = context;
        }

        // GET: api/Categories
        // Retrieve all categories.
        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = new List<object>();

            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT CategoryID, CategoryName, Description, Type
                FROM Category
                ORDER BY CategoryName;";

            using SqlCommand command = new SqlCommand(sql, connection);

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add(new
                {
                    CategoryID = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    Description = reader.IsDBNull(2)
                        ? null
                        : reader.GetString(2),
                    Type = reader.GetString(3)
                });
            }

            return Ok(categories);
        }

        // GET: api/Categories/1
        // Retrieve one category by its ID.
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCategory(int id)
        {
            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                SELECT CategoryID, CategoryName, Description, Type
                FROM Category
                WHERE CategoryID = @CategoryID;";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = id;

            using SqlDataReader reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            var category = new
            {
                CategoryID = reader.GetInt32(0),
                CategoryName = reader.GetString(1),
                Description = reader.IsDBNull(2)
                    ? null
                    : reader.GetString(2),
                Type = reader.GetString(3)
            };

            return Ok(category);
        }

        // POST: api/Categories
        // Create a new category.
        [HttpPost]
        public async Task<IActionResult> CreateCategory(
            [FromBody] CategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.CategoryName) ||
                string.IsNullOrWhiteSpace(request.Type))
            {
                return BadRequest(new
                {
                    message = "CategoryName and Type are required."
                });
            }

            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                INSERT INTO Category (CategoryName, Description, Type)
                OUTPUT INSERTED.CategoryID
                VALUES (@CategoryName, @Description, @Type);";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@CategoryName", SqlDbType.VarChar, 100)
                .Value = request.CategoryName.Trim();

            command.Parameters.Add("@Description", SqlDbType.VarChar, -1)
                .Value = (object?)request.Description ?? DBNull.Value;

            command.Parameters.Add("@Type", SqlDbType.VarChar, 20)
                .Value = request.Type.Trim();

            int newCategoryId =
                (int)await command.ExecuteScalarAsync();

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = newCategoryId },
                new
                {
                    categoryID = newCategoryId,
                    message = "Category created successfully."
                });
        }

        // PUT: api/Categories/1
        // Update an existing category.
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCategory(
            int id,
            [FromBody] CategoryRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (string.IsNullOrWhiteSpace(request.CategoryName) ||
                string.IsNullOrWhiteSpace(request.Type))
            {
                return BadRequest(new
                {
                    message = "CategoryName and Type are required."
                });
            }

            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                UPDATE Category
                SET CategoryName = @CategoryName,
                    Description = @Description,
                    Type = @Type
                WHERE CategoryID = @CategoryID;";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = id;

            command.Parameters.Add("@CategoryName", SqlDbType.VarChar, 100)
                .Value = request.CategoryName.Trim();

            command.Parameters.Add("@Description", SqlDbType.VarChar, -1)
                .Value = (object?)request.Description ?? DBNull.Value;

            command.Parameters.Add("@Type", SqlDbType.VarChar, 20)
                .Value = request.Type.Trim();

            int rowsAffected = await command.ExecuteNonQueryAsync();

            if (rowsAffected == 0)
            {
                return NotFound(new
                {
                    message = "Category not found."
                });
            }

            return Ok(new
            {
                message = "Category updated successfully."
            });
        }

        // DELETE: api/Categories/1
        // Delete a category.
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            using SqlConnection connection = _context.GetConnection();
            await connection.OpenAsync();

            string sql = @"
                DELETE FROM Category
                WHERE CategoryID = @CategoryID;";

            using SqlCommand command = new SqlCommand(sql, connection);

            command.Parameters.Add("@CategoryID", SqlDbType.Int).Value = id;

            try
            {
                int rowsAffected = await command.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    return NotFound(new
                    {
                        message = "Category not found."
                    });
                }
            }
            catch (SqlException)
            {
                return Conflict(new
                {
                    message = "This category may be linked to an event. Remove or handle the related event-category records before deleting it."
                });
            }

            return Ok(new
            {
                message = "Category deleted successfully."
            });
        }
    }

    public class CategoryRequest
    {
        public string CategoryName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string Type { get; set; } = string.Empty;
    }
}