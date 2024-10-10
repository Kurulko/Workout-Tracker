using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Tests;

public static class IdentityHelper
{
    public static RoleManager<IdentityRole> GetRoleManager(WorkoutDbContext db)
    {
        var roleStore = new RoleStore<IdentityRole>(db);
        return new RoleManager<IdentityRole>(
                   roleStore,
                   Array.Empty<IRoleValidator<IdentityRole>>(),
                   new UpperInvariantLookupNormalizer(),
                   Mock.Of<IdentityErrorDescriber>(),
                   Mock.Of<ILogger<RoleManager<IdentityRole>>>());
    }

    public static UserManager<User> GetUserManager(WorkoutDbContext db)
    {
        var userStore = new UserStore<User>(db);
        return new UserManager<User>(
                    userStore,
                    Mock.Of<IOptions<IdentityOptions>>(),
                    Mock.Of<IPasswordHasher<User>>(),
                    Array.Empty<IUserValidator<User>>(),
                    Array.Empty<IPasswordValidator<User>>(),
                    new UpperInvariantLookupNormalizer(),
                    Mock.Of<IdentityErrorDescriber>(),
                    Mock.Of<IServiceProvider>(),
                    Mock.Of<ILogger<UserManager<User>>>());
    }
    
    public static SignInManager<User> GetSignInManager(WorkoutDbContext db, IHttpContextAccessor httpContextAccessor)
    {
        var userManager = GetUserManager(db);
        return new SignInManager<User>(
            userManager,
            httpContextAccessor,
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<User>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<User>>());
    }
}