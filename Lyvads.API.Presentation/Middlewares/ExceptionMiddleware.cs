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

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment hostingEnvironment)
    {
        _next = next;
        _logger = logger;
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

    private async Task HandleException(HttpContext context, Exception ex, string errorMessage, HttpStatusCode statusCode)
    {
        // Log the exception with detailed information
        _logger.LogError(ex, ex.Message);

        var response = context.Response;
        response.ContentType = "application/json";
        response.StatusCode = (int)statusCode; // Set the response status code explicitly

        // Return detailed error in non-production environments
        var errorDetails = _isProdEnv
            ? errorMessage
            : $"{errorMessage} | Exception: {ex.Message}";  // Include exception message only in non-production

        var result = JsonSerializer.Serialize(ResponseDto<object>.Failure(
            new Error[] { new("Server.Error", errorDetails) }, (int)statusCode));

        await response.WriteAsync(result);
    }
}
