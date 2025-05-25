using Microsoft.AspNetCore.Http;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Settings;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services.Auth;

public class ImpersonationServiceValidator
{
    readonly UserValidator userValidator;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly SessionKeys sessionKeys;

    public ImpersonationServiceValidator (
        IHttpContextAccessor httpContextAccessor,
        UserValidator userValidator,
        SessionKeys sessionKeys
    )
    {
        this.httpContextAccessor = httpContextAccessor;
        this.userValidator = userValidator;
        this.sessionKeys = sessionKeys;
    }

    HttpContext? HttpContext => httpContextAccessor.HttpContext;

    public async Task ValidateImpersonateAsync(string userId)
    {
        ThrowIfNotAuthenticated();

        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateRevertAsync()
    {
        ThrowIfNotAuthenticated();

        var originalUserId = HttpContext!.Session.GetString(sessionKeys.OriginalUserId);
        ArgumentValidator.ThrowIfIdNullOrEmpty(originalUserId, "Original User ID");

        await userValidator.EnsureExistsAsync(originalUserId!);
    }

    void ThrowIfNotAuthenticated()
    {
        if (HttpContext?.User?.Identity is null || !HttpContext.User.Identity.IsAuthenticated)
            throw new UnauthorizedException("Original user not authenticated");
    }
}
