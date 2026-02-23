using Microsoft.AspNetCore.Mvc;

namespace OksiMin.Api.Controllers
{
    /// <summary>
    /// Test endpoints for middleware verification (DELETE IN PRODUCTION)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Test correlation ID tracking
        /// </summary>
        [HttpGet("correlation")]
        public IActionResult TestCorrelation()
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

            _logger.LogInformation("Testing correlation ID: {CorrelationId}", correlationId);

            return Ok(new
            {
                correlationId,
                message = "Check the response headers for X-Correlation-ID",
                timestamp = DateTime.UtcNow
            });
        }

        /// <summary>
        /// Test exception handler (throws exception)
        /// </summary>
        [HttpGet("exception")]
        public IActionResult TestException()
        {
            _logger.LogInformation("About to throw test exception");
            throw new InvalidOperationException("This is a test exception to verify global exception handling");
        }

        /// <summary>
        /// Test different exception types
        /// </summary>
        [HttpGet("exception/{type}")]
        public IActionResult TestExceptionType(string type)
        {
            _logger.LogInformation("Testing exception type: {ExceptionType}", type);

            throw type.ToLower() switch
            {
                "notfound" => new KeyNotFoundException("Test resource not found"),
                "unauthorized" => new UnauthorizedAccessException("Test unauthorized access"),
                "argument" => new ArgumentException("Test invalid argument"),
                "timeout" => new TimeoutException("Test timeout"),
                _ => new Exception("Test generic exception")
            };
        }

        /// <summary>
        /// Test logging with correlation ID
        /// </summary>
        [HttpGet("logging")]
        public IActionResult TestLogging()
        {
            var correlationId = HttpContext.Items["CorrelationId"]?.ToString();

            _logger.LogDebug("This is a DEBUG log with CorrelationId: {CorrelationId}", correlationId);
            _logger.LogInformation("This is an INFO log with CorrelationId: {CorrelationId}", correlationId);
            _logger.LogWarning("This is a WARNING log with CorrelationId: {CorrelationId}", correlationId);

            return Ok(new
            {
                message = "Check Seq logs - all logs should have the same CorrelationId",
                correlationId
            });
        }
    }
}
