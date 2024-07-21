using System.Net;
using System.Text.Json;
using Lyvads.API.Presentation.Dtos;
using Lyvads.Application.Dtos;

namespace Lyvads.API.Presentation.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly bool _isProdEnv;

    public ExceptionMiddleware(RequestDelegate next,
        ILogger<ExceptionMiddleware> logger, IHostEnvironment hostingEnvironment)
    {
        _logger = logger;
        _next = next;
        _isProdEnv = hostingEnvironment.IsProduction();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            await HandleException(context, ex, "Invalid argument.", HttpStatusCode.BadRequest);
        }
        catch (UnauthorizedAccessException ex)
        {
            await HandleException(context, ex, "Unauthorized access.", HttpStatusCode.Unauthorized);
        }
        catch (Exception ex)
        {
            await HandleException(context, ex, "An unexpected error occurred.", HttpStatusCode.InternalServerError);
        }
    }

    private async Task HandleException(HttpContext context, Exception ex, string errorMessage,
        HttpStatusCode statusCode)
    {
        _logger.LogError(ex, ex.Message);

        var response = context.Response;
        response.ContentType = "application/json";

        var result =
            JsonSerializer.Serialize(ResponseDto<object>.Failure(new Error[] { new("Server.Error", errorMessage) },
                (int)statusCode));

        await response.WriteAsync(result);
    }
}