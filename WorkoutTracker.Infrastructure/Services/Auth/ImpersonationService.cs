using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Settings;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.Interfaces.Services.Auth;
using WorkoutTracker.Infrastructure.Auth;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Validators.Services.Auth;

namespace WorkoutTracker.Infrastructure.Services.Auth;

internal class ImpersonationService : IImpersonationService
{
    readonly IUserRepository userRepository;
    readonly SignInManager<User> signInManager;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly JwtHandler jwtHandler;
    readonly SessionKeysOptions sessionKeysOptions;
    readonly ImpersonationServiceValidator impersonationServiceValidator;

    public ImpersonationService(
        IUserRepository userRepository,
        SignInManager<User> signInManager,
        IHttpContextAccessor httpContextAccessor,
        JwtHandler jwtHandler,
        SessionKeysOptions sessionKeysOptions,
        ImpersonationServiceValidator impersonationServiceValidator
    )
    {
        this.userRepository = userRepository;
        this.signInManager = signInManager;
        this.httpContextAccessor = httpContextAccessor;
        this.jwtHandler = jwtHandler;
        this.sessionKeysOptions = sessionKeysOptions;
        this.impersonationServiceValidator = impersonationServiceValidator;
    }

    HttpContext HttpContext => httpContextAccessor.HttpContext!;

    public async Task<TokenModel> ImpersonateAsync(string userId)
    {
        await impersonationServiceValidator.ValidateImpersonateAsync(userId);

        User userToImpersonate = (await userRepository.GetUserByIdAsync(userId))!;

        string originalUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        HttpContext.Session.SetString(sessionKeysOptions.OriginalUserId, originalUserId);

        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(userToImpersonate, isPersistent: false);

        var token = await jwtHandler.GenerateJwtTokenAsync(userToImpersonate);
        return token;
    }

    public async Task<TokenModel> RevertAsync()
    {
        await impersonationServiceValidator.ValidateRevertAsync();

        string originalUserId = HttpContext.Session.GetString(sessionKeysOptions.OriginalUserId)!;
        User originalUser = (await userRepository.GetUserByIdAsync(originalUserId))!;

        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(originalUser, isPersistent: false);
        HttpContext.Session.Remove(sessionKeysOptions.OriginalUserId);

        var token = await jwtHandler.GenerateJwtTokenAsync(originalUser);
        return token;
    }

    public bool IsImpersonating()
    {
        string? originalUserId = HttpContext.Session.GetString(sessionKeysOptions.OriginalUserId);
        return !string.IsNullOrEmpty(originalUserId);
    }
}
