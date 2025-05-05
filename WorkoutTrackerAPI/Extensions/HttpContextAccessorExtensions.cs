using System.Security.Claims;

namespace WorkoutTracker.API.Extensions;

public static class HttpContextAccessorExtensions
{
    public static string? GetUserId(this IHttpContextAccessor httpContextAccessor)
        => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
