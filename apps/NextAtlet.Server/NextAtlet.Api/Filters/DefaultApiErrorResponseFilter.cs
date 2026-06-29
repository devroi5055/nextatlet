using Microsoft.OpenApi;
using NextAtlet.Application.Common.Errors;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NextAtlet.Api.Filters
{
    public class DefaultApiErrorResponseFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses?["default"] = new OpenApiResponse
            {
                Description = "Error response",
                Content = new Dictionary<string, OpenApiMediaType>
                {
                    ["application/json"] = new OpenApiMediaType
                    {
                        Schema = context.SchemaGenerator.GenerateSchema(typeof(ApiError), context.SchemaRepository)
                    }
                }
            };
        }
    }
}
