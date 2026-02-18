using Microsoft.AspNetCore.Mvc;
using OksiMin.Application.DTOs;
using OksiMin.Application.Interfaces;

namespace OksiMin.Api.Controllers
{
    /// <summary>
    /// Public category endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(
            ICategoryService categoryService,
            ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        /// <returns>List of all categories</returns>
        /// <response code="200">Categories retrieved successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCategories()
        {
            _logger.LogDebug("Fetching all categories");

            var result = await _categoryService.GetAllCategoriesAsync();

            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to retrieve categories. Error: {Error}", result.Error);
                return StatusCode(500, new ErrorResponse
                {
                    Message = result.Error ?? "Failed to retrieve categories"
                });
            }

            _logger.LogInformation("Retrieved {Count} categories", result.Data!.Count);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get category by ID
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category details</returns>
        /// <response code="200">Category found</response>
        /// <response code="404">Category not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            _logger.LogDebug("Fetching category {CategoryId}", id);

            var result = await _categoryService.GetCategoryByIdAsync(id);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Category {CategoryId} not found", id);
                return NotFound(new ErrorResponse
                {
                    Message = result.Error ?? "Category not found"
                });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Get category details with statistics
        /// </summary>
        /// <param name="id">Category ID</param>
        /// <returns>Category with place and submission counts</returns>
        /// <response code="200">Category details retrieved</response>
        /// <response code="404">Category not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}/details")]
        [ProducesResponseType(typeof(CategoryDetailResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategoryDetail(int id)
        {
            _logger.LogDebug("Fetching category details for {CategoryId}", id);

            var result = await _categoryService.GetCategoryDetailAsync(id);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Category {CategoryId} not found", id);
                return NotFound(new ErrorResponse
                {
                    Message = result.Error ?? "Category not found"
                });
            }

            return Ok(result.Data);
        }
    }
}
