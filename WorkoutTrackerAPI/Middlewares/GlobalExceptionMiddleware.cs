using Microsoft.IdentityModel.Tokens;
using WorkoutTracker.Application.Common.Exceptions;

namespace WorkoutTracker.API.Middlewares;

public class GlobalExceptionMiddleware
{
    readonly RequestDelegate next;
    readonly ILogger<GlobalExceptionMiddleware> logger;
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex) when (ex is IWorkoutException)
        {
            int statusCode = ex switch
            {
                NotFoundException => StatusCodes.Status404NotFound,
                UnauthorizedException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status400BadRequest
            };
            string errorMessage = ex.Message;

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(errorMessage);
        }
        catch (Exception ex)
        {

            int statusCode = ex switch
            {
                ArgumentException or InvalidOperationException =>
                    StatusCodes.Status400BadRequest,
                FileNotFoundException => StatusCodes.Status404NotFound,
                SecurityTokenException or UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                _ => StatusCodes.Status500InternalServerError
            };

            string errorMessage = "Unhandled exception occurred.";
            logger.LogError(ex, errorMessage);

            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(errorMessage);
        }
    }
}
