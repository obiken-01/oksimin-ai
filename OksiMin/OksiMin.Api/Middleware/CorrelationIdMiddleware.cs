namespace OksiMin.Api.Middleware
{
    /// <summary>
    /// Middleware to handle correlation IDs for request tracking
    /// </summary>
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;
        private const string CorrelationIdHeader = "X-Correlation-ID";

        public CorrelationIdMiddleware(
            RequestDelegate next,
            ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Get correlation ID from request header or generate new one
            var correlationId = GetOrCreateCorrelationId(context);

            // Add to response headers so client can track the request
            context.Response.Headers.TryAdd(CorrelationIdHeader, correlationId);

            // Add to HttpContext items for access throughout the request pipeline
            context.Items["CorrelationId"] = correlationId;

            // Add to Serilog logging context (will appear in all logs for this request)
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
            {
                _logger.LogDebug(
                    "Request started with CorrelationId: {CorrelationId}",
                    correlationId);

                try
                {
                    await _next(context);
                }
                finally
                {
                    _logger.LogDebug(
                        "Request completed with CorrelationId: {CorrelationId}",
                        correlationId);
                }
            }
        }

        private string GetOrCreateCorrelationId(HttpContext context)
        {
            // Check if client provided a correlation ID
            if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var correlationId) &&
                !string.IsNullOrWhiteSpace(correlationId))
            {
                return correlationId.ToString();
            }

            // Generate new correlation ID
            return Guid.NewGuid().ToString();
        }
    }
}
