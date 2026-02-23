using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace OksiMin.Api.Filters
{
    /// <summary>
    /// Adds X-Correlation-ID header to all Swagger operations
    /// </summary>
    public class CorrelationIdOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Initialize parameters list if null
            operation.Parameters ??= new List<IOpenApiParameter>();

            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "X-Correlation-ID",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Description = "UUID format"
                },
                Description = "Optional correlation ID for request tracking. If not provided, one will be automatically generated."
            });
        }
    }
}
