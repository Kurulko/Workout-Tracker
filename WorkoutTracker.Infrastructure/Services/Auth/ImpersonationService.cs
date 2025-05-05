using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.Interfaces.Services.Auth;
using WorkoutTracker.Infrastructure.Auth;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;

namespace WorkoutTracker.Infrastructure.Services.Auth;

internal class ImpersonationService : IImpersonationService
{
    readonly IUserRepository userRepository;
    readonly SignInManager<User> signInManager;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly JwtHandler jwtHandler;
    public ImpersonationService(IUserRepository userRepository, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor, JwtHandler jwtHandler)
    {
        this.userRepository = userRepository;
        this.signInManager = signInManager;
        this.httpContextAccessor = httpContextAccessor;
        this.jwtHandler = jwtHandler;
    }

    const string originalUserIdSessionKey = "OriginalUserId";
    HttpContext HttpContext => httpContextAccessor.HttpContext!;

    public virtual async Task<TokenModel> ImpersonateAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullOrEmptyException("User ID");

        User userToImpersonate = await userRepository.GetUserByIdAsync(userId) ?? throw NotFoundException.NotFoundExceptionByID("User to impersonate", userId);

        string originalUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("Original user not authenticated"); 
        
        HttpContext.Session.SetString(originalUserIdSessionKey, originalUserId);

        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(userToImpersonate, isPersistent: false);

        var token = await jwtHandler.GenerateJwtTokenAsync(userToImpersonate);
        return token;
    }

    public virtual async Task<TokenModel> RevertAsync()
    {
        string? originalUserId = HttpContext.Session.GetString(originalUserIdSessionKey);
        if (string.IsNullOrEmpty(originalUserId))
            throw new ArgumentNullOrEmptyException("Original User ID");

        User? originalUser = await userRepository.GetUserByIdAsync(originalUserId) ?? throw NotFoundException.NotFoundExceptionByID("Original User", originalUserId);

        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(originalUser, isPersistent: false);
        HttpContext.Session.Remove(originalUserIdSessionKey);

        var token = await jwtHandler.GenerateJwtTokenAsync(originalUser);
        return token;
    }

    public virtual bool IsImpersonating()
    {
        try
        {
            string? originalUserId = HttpContext.Session.GetString(originalUserIdSessionKey);
            return !string.IsNullOrEmpty(originalUserId);
        }
        catch
        {
            return false;
        }
    }
}
