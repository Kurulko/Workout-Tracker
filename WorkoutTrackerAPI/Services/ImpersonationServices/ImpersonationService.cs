﻿using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;

namespace WorkoutTrackerAPI.Services.ImpersonationServices;

public class ImpersonationService : IImpersonationService
{
    readonly UserManager<User> userManager;
    readonly SignInManager<User> signInManager;
    readonly IHttpContextAccessor httpContextAccessor;
    public ImpersonationService(UserManager<User> userManager, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor)
    {
        this.userManager = userManager;
        this.signInManager = signInManager;
        this.httpContextAccessor = httpContextAccessor;
    }

    const string OriginalUserIdSessionKey = "OriginalUserId";
    HttpContext HttpContext => httpContextAccessor.HttpContext!;

    public async Task ImpersonateAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullOrEmptyException("User ID");

        User? userToImpersonate = await userManager.FindByIdAsync(userId) ?? throw new NotFoundException("User to impersonate");

        string originalUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        HttpContext.Session.SetString(OriginalUserIdSessionKey, originalUserId);

        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(userToImpersonate, isPersistent: false);
    }

    public async Task RevertAsync()
    {
        string? originalUserId = HttpContext.Session.GetString(OriginalUserIdSessionKey);
        if (string.IsNullOrEmpty(originalUserId))
            throw new NotFoundException("Original User ID");

        User? originalUser = await userManager.FindByIdAsync(originalUserId) ?? throw new NotFoundException("Original User");

        await signInManager.SignOutAsync();
        await signInManager.SignInAsync(originalUser, isPersistent: false);
        HttpContext.Session.Remove(OriginalUserIdSessionKey);
    }

    public bool IsImpersonating()
    {
        try
        {
            string? originalUserId = HttpContext.Session.GetString(OriginalUserIdSessionKey);
            return !string.IsNullOrEmpty(originalUserId);
        }
        catch
        {
            return false;
        }
    }
}
