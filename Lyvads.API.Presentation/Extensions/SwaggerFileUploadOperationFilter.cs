using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lyvads.API.Presentation.Extensions;

public class SwaggerFileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Handle file uploads
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(IFormFile));

        foreach (var param in fileParams)
        {
            var fileParam = new OpenApiParameter
            {
                Name = param.Name,
                In = ParameterLocation.Query,
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                }
            };

            operation.Parameters.Add(fileParam);
        }
    }
}