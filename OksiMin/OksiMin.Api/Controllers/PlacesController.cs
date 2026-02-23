using Microsoft.AspNetCore.Mvc;
using OksiMin.Application.DTOs;
using OksiMin.Application.Interfaces;

namespace OksiMin.Api.Controllers
{
    /// <summary>
    /// Public place browsing endpoints
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PlacesController : ControllerBase
    {
        private readonly IPlaceService _placeService;
        private readonly ILogger<PlacesController> _logger;

        public PlacesController(
            IPlaceService placeService,
            ILogger<PlacesController> logger)
        {
            _placeService = placeService;
            _logger = logger;
        }

        /// <summary>
        /// Get all approved places
        /// </summary>
        /// <returns>List of all approved places</returns>
        /// <response code="200">Places retrieved successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet]
        [ProducesResponseType(typeof(List<PlaceListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllPlaces()
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

            _logger.LogDebug("Fetching all approved places. CorrelationId: {CorrelationId}.",
                correlationId);

            var result = await _placeService.GetAllPlacesAsync();

            if (!result.IsSuccess)
            {
                _logger.LogError("Failed to retrieve places. Error: {Error}. CorrelationId: {CorrelationId}.", 
                    result.Error,
                    correlationId);
                return StatusCode(500, new ErrorResponse
                {
                    Message = result.Error ?? "Failed to retrieve places"
                });
            }

            _logger.LogInformation("Retrieved {Count} places. CorrelationId: {CorrelationId}.", 
                result.Data!.Count, 
                correlationId);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get place by ID
        /// </summary>
        /// <param name="id">Place ID</param>
        /// <returns>Place details</returns>
        /// <response code="200">Place found</response>
        /// <response code="404">Place not found</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PlaceResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPlaceById(int id)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

            _logger.LogDebug("Fetching place {PlaceId}. CorrelationId: {CorrelationId}.", 
                id, correlationId);

            var result = await _placeService.GetPlaceByIdAsync(id);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Place {PlaceId} not found. CorrelationId: {CorrelationId}.", 
                    id, 
                    correlationId);
                return NotFound(new ErrorResponse
                {
                    Message = result.Error ?? "Place not found"
                });
            }

            return Ok(result.Data);
        }

        /// <summary>
        /// Get places by municipality
        /// </summary>
        /// <param name="municipality">Municipality name (e.g., "Sablayan", "Mamburao")</param>
        /// <returns>List of places in the specified municipality</returns>
        /// <response code="200">Places retrieved successfully</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("municipality/{municipality}")]
        [ProducesResponseType(typeof(List<PlaceListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPlacesByMunicipality(string municipality)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

            _logger.LogDebug("Fetching places in municipality: {Municipality}. CorrelationId: {CorrelationId}.", 
                municipality, 
                correlationId);

            var result = await _placeService.GetPlacesByMunicipalityAsync(municipality);

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    "Failed to retrieve places for {Municipality}. Error: {Error}. CorrelationId: {CorrelationId}.",
                    municipality,
                    result.Error,
                    correlationId);

                return StatusCode(500, new ErrorResponse
                {
                    Message = result.Error ?? "Failed to retrieve places"
                });
            }

            _logger.LogInformation(
                "Retrieved {Count} places in {Municipality}. CorrelationId: {CorrelationId}.",
                result.Data!.Count,
                municipality,
                correlationId);

            return Ok(result.Data);
        }

        /// <summary>
        /// Get places by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <returns>List of places in the specified category</returns>
        /// <response code="200">Places retrieved successfully</response>
        /// <response code="400">Invalid category ID</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(typeof(List<PlaceListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetPlacesByCategory(int categoryId)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

            _logger.LogDebug("Fetching places in category: {CategoryId}. CorrelationId: {CorrelationId}.", 
                categoryId, 
                correlationId);

            var result = await _placeService.GetPlacesByCategoryAsync(categoryId);

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    "Failed to retrieve places for category {CategoryId}. Error: {Error}. CorrelationId: {CorrelationId}.",
                    categoryId,
                    result.Error, 
                    correlationId);

                return BadRequest(new ErrorResponse
                {
                    Message = result.Error ?? "Failed to retrieve places"
                });
            }

            _logger.LogInformation(
                "Retrieved {Count} places in category {CategoryId}. CorrelationId: {CorrelationId}.",
                result.Data!.Count,
                categoryId, 
                correlationId);

            return Ok(result.Data);
        }

        /// <summary>
        /// Search places by name, description, or tags
        /// </summary>
        /// <param name="q">Search query</param>
        /// <returns>List of matching places</returns>
        /// <response code="200">Search completed successfully</response>
        /// <response code="400">Empty search term</response>
        /// <response code="500">Internal server error</response>
        [HttpGet("search")]
        [ProducesResponseType(typeof(List<PlaceListResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> SearchPlaces([FromQuery] string q)
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

            if (string.IsNullOrWhiteSpace(q))
            {
                return BadRequest(new ErrorResponse
                {
                    Message = "Search term cannot be empty"
                });
            }

            _logger.LogDebug("Searching places with query: {SearchQuery}. CorrelationId: {CorrelationId}.", 
                q, 
                correlationId);

            var result = await _placeService.SearchPlacesAsync(q);

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    "Failed to search places with query {SearchQuery}. Error: {Error}. CorrelationId: {CorrelationId}.",
                    q,
                    result.Error, 
                    correlationId);

                return StatusCode(500, new ErrorResponse
                {
                    Message = result.Error ?? "Failed to search places"
                });
            }

            _logger.LogInformation(
                "Found {Count} places matching query: {SearchQuery}. CorrelationId: {CorrelationId}.",
                result.Data!.Count,
                q, 
                correlationId);

            return Ok(result.Data);
        }
    }
}
