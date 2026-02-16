using Microsoft.AspNetCore.Mvc;

namespace OksiMin.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet]
    public IActionResult Get()
    {
        _logger.LogInformation("Health check endpoint called from {RemoteIP}",
            HttpContext.Connection.RemoteIpAddress?.ToString());

        return Ok(new
        {
            status = "healthy",
            application = "OksiMin.Api",
            timestamp = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        });
    }

    /// <summary>
    /// Test different log levels
    /// </summary>
    [HttpGet("test-logging")]
    public IActionResult TestLogging()
    {
        _logger.LogTrace("This is a TRACE log");
        _logger.LogDebug("This is a DEBUG log");
        _logger.LogInformation("This is an INFORMATION log with {PropertyName}", "structured data");
        _logger.LogWarning("This is a WARNING log");
        _logger.LogError("This is an ERROR log");

        return Ok(new { message = "Check logs for different levels" });
    }

    /// <summary>
    /// Test exception logging
    /// </summary>
    [HttpGet("test-exception")]
    public IActionResult TestException()
    {
        try
        {
            throw new InvalidOperationException("This is a test exception");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test exception caught. ErrorMessage: {ErrorMessage}", ex.Message);
            return StatusCode(500, new { message = "Exception logged successfully" });
        }
    }
}