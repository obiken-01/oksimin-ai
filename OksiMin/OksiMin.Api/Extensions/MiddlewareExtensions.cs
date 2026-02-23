using OksiMin.Api.Middleware;

namespace OksiMin.Api.Extensions
{
    /// <summary>
    /// Extension methods for registering custom middleware
    /// </summary>
    public static class MiddlewareExtensions
    {
        /// <summary>
        /// Adds correlation ID middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }

        /// <summary>
        /// Adds global exception handler middleware to the pipeline
        /// </summary>
        public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}
