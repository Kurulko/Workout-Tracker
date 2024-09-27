using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace WorkoutTrackerAPI.Extentions;

public static class HttpContextAccessorExtentions
{
    public static string? GetUserId(this IHttpContextAccessor httpContextAccessor)
        => httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
}
