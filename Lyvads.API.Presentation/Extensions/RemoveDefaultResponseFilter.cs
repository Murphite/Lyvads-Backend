using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lyvads.API.Presentation.Extensions;

public class RemoveDefaultResponseFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Remove unwanted default responses
        operation.Responses.Remove("200");
        operation.Responses.Remove("400");
        operation.Responses.Remove("500");
    }
}