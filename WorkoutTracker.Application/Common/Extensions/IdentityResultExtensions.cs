using Microsoft.AspNetCore.Identity;

namespace WorkoutTracker.Application.Common.Extensions;

public static class IdentityResultExtensions
{
    public static string IdentityErrorsToString(this IdentityResult identityResult)
        => string.Join("; ", identityResult.Errors.Select(e => e.Description));
}
