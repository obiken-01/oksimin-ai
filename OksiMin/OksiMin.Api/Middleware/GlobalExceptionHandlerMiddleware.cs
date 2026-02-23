using System.Text.Json;

namespace OksiMin.Api.Middleware
{
    /// <summary>
    /// Middleware to handle all unhandled exceptions globally
    /// </summary>
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandlerMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlerMiddleware> logger,
            IHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";

            // Log the exception with full context
            _logger.LogError(
                exception,
                "Unhandled exception occurred. " +
                "CorrelationId: {CorrelationId}, " +
                "Path: {Path}, " +
                "Method: {Method}, " +
                "QueryString: {QueryString}, " +
                "RemoteIP: {RemoteIP}",
                correlationId,
                context.Request.Path,
                context.Request.Method,
                context.Request.QueryString,
                context.Connection.RemoteIpAddress?.ToString());

            // Determine status code based on exception type
            var (statusCode, message) = GetResponseDetails(exception);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var response = new GlobalErrorResponse
            {
                CorrelationId = correlationId,
                StatusCode = statusCode,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Path = context.Request.Path
            };

            // Include details only in development environment
            if (_environment.IsDevelopment())
            {
                response.Details = exception.Message;
                response.StackTrace = exception.StackTrace;
                response.InnerException = exception.InnerException?.Message;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = _environment.IsDevelopment()
            };

            var json = JsonSerializer.Serialize(response, options);
            await context.Response.WriteAsync(json);
        }

        private static (int statusCode, string message) GetResponseDetails(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => (StatusCodes.Status400BadRequest, "Invalid request data"),
                ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request data"),
                InvalidOperationException => (StatusCodes.Status400BadRequest, "Invalid operation"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized access"),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
                NotImplementedException => (StatusCodes.Status501NotImplemented, "Feature not implemented"),
                TimeoutException => (StatusCodes.Status408RequestTimeout, "Request timeout"),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred")
            };
        }
    }

    /// <summary>
    /// Global error response model
    /// </summary>
    public class GlobalErrorResponse
    {
        public string CorrelationId { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Path { get; set; } = string.Empty;

        // Development-only properties
        public string? Details { get; set; }
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
    }
}
