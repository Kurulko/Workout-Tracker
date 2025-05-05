using Microsoft.AspNetCore.Identity;

namespace WorkoutTracker.Infrastructure.Identity.Extensions;

public static class IdentityResultExtensions
{
    public static IdentityResult Failed(Exception ex)
        => IdentityResult.Failed(
                new IdentityError() { Description = ex.Message }
            );

    public static IdentityResult Failed(string code, Exception ex)
        => IdentityResult.Failed(
                new IdentityError() { Code = code, Description = ex.Message }
            );

    public static IdentityResult Failed(string message)
        => IdentityResult.Failed(
                new IdentityError() { Description = message }
            );

    public static IdentityResult Failed(string code, string message)
        => IdentityResult.Failed(
                new IdentityError() { Code = code, Description = message }
            );

    public static bool ErrorExists(this IdentityResult identityResult, string errorMessage)
    {
        if (identityResult.Succeeded)
            return false;

        return identityResult.Errors.Any(e => e.Description.Contains(errorMessage));
    }
}
