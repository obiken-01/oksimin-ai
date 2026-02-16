using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OksiMin.Infrastructure.Data;

namespace OksiMin.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseTestController : ControllerBase
    {
        private readonly OksiMinDbContext _context;
        private readonly ILogger<DatabaseTestController> _logger;

        public DatabaseTestController(
            OksiMinDbContext context,
            ILogger<DatabaseTestController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Test database connection
        /// </summary>
        [HttpGet("connection")]
        [ProducesResponseType(typeof(ConnectionTestResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();

                _logger.LogInformation("Database connection test: {CanConnect}", canConnect);

                return Ok(new ConnectionTestResponse
                {
                    CanConnect = canConnect,
                    Message = canConnect ? "Database connection successful!" : "Cannot connect to database",
                    DatabaseName = _context.Database.GetDbConnection().Database
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database connection test failed");
                return StatusCode(500, new ConnectionTestResponse
                {
                    CanConnect = false,
                    Message = $"Error: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// Get category count (tests data access)
        /// </summary>
        [HttpGet("categories/count")]
        [ProducesResponseType(typeof(CategoryCountResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategoryCount()
        {
            try
            {
                var count = await _context.Categories.CountAsync();

                _logger.LogInformation("Category count: {Count}", count);

                return Ok(new CategoryCountResponse
                {
                    Count = count,
                    Message = $"Found {count} categories in database"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get category count");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all categories (tests seed data)
        /// </summary>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories
                    .OrderBy(c => c.Id)
                    .Select(c => new CategoryDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Description = c.Description
                    })
                    .ToListAsync();

                _logger.LogInformation("Retrieved {Count} categories", categories.Count);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get categories");
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    public class ConnectionTestResponse
    {
        public bool CanConnect { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? DatabaseName { get; set; }
    }

    public class CategoryCountResponse
    {
        public int Count { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
