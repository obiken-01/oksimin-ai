using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OksiMin.Application.DTOs;
using OksiMin.Application.Interfaces;

namespace OksiMin.Api.Controllers
{
    /// <summary>
    /// Public submission endpoints for anonymous users
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class SubmissionsController : ControllerBase
    {
        private readonly ISubmissionService _submissionService;
        private readonly IValidator<CreateSubmissionRequest> _validator;
        private readonly ILogger<SubmissionsController> _logger;

        public SubmissionsController(
            ISubmissionService submissionService,
            IValidator<CreateSubmissionRequest> validator,
            ILogger<SubmissionsController> logger)
        {
            _submissionService = submissionService;
            _validator = validator;
            _logger = logger;
        }

        /// <summary>
        /// Submit a new place for review (anonymous)
        /// </summary>
        /// <param name="request">Place submission details</param>
        /// <returns>Created submission with pending status</returns>
        /// <response code="201">Submission created successfully</response>
        /// <response code="400">Invalid request data</response>
        /// <response code="500">Internal server error</response>
        [HttpPost]
        [ProducesResponseType(typeof(SubmissionResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateSubmission([FromBody] CreateSubmissionRequest request)
        {
            _logger.LogInformation(
                "Received submission request for {Name} in {Municipality}",
                request.Name,
                request.Municipality);

            // Validate request
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                _logger.LogWarning(
                    "Validation failed for submission {Name}. Errors: {@ValidationErrors}",
                    request.Name,
                    validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

                return BadRequest(new ValidationErrorResponse
                {
                    Message = "Validation failed",
                    Errors = validationResult.Errors.Select(e => new ValidationError
                    {
                        Field = e.PropertyName,
                        Message = e.ErrorMessage
                    }).ToList()
                });
            }

            // Get submitter IP address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Create submission
            var result = await _submissionService.CreateSubmissionAsync(request, ipAddress);

            if (!result.IsSuccess)
            {
                _logger.LogError(
                    "Failed to create submission for {Name}. Error: {Error}",
                    request.Name,
                    result.Error);

                return BadRequest(new ErrorResponse
                {
                    Message = result.Error ?? "Failed to create submission"
                });
            }

            _logger.LogInformation(
                "Submission created successfully. SubmissionId: {SubmissionId}, Name: {Name}",
                result.Data!.Id,
                result.Data.Name);

            return CreatedAtAction(
                nameof(CreateSubmission),
                new { id = result.Data.Id },
                result.Data);
        }

        /// <summary>
        /// Get pending submissions count (public endpoint for transparency)
        /// </summary>
        /// <returns>Number of pending submissions</returns>
        /// <response code="200">Count retrieved successfully</response>
        [HttpGet("pending/count")]
        [ProducesResponseType(typeof(CountResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingCount()
        {
            var result = await _submissionService.GetPendingCountAsync();

            if (!result.IsSuccess)
            {
                return StatusCode(500, new ErrorResponse
                {
                    Message = result.Error ?? "Failed to retrieve pending count"
                });
            }

            return Ok(new CountResponse
            {
                Count = result.Data,
                Message = $"{result.Data} submission(s) pending review"
            });
        }
    }

    #region Response DTOs

    public class ValidationErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<ValidationError> Errors { get; set; } = new();
    }

    public class ValidationError
    {
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }

    public class CountResponse
    {
        public int Count { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    #endregion
}
