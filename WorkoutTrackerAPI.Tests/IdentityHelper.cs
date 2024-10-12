using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Security.Claims;
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

    public static Mock<HttpContext> GetMockHttpContext()
    {
        var mockHttpContext = new Mock<HttpContext>();

        var session = new DistributedSession(
            new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())),
            "session-id",
            TimeSpan.FromMinutes(20),
            TimeSpan.FromMinutes(20),
            () => true,
            new NullLoggerFactory(),
            true
        );

        mockHttpContext.Setup(x => x.Session).Returns(session);
        return mockHttpContext;
    }

    public static Mock<IHttpContextAccessor> GetMockHttpContextAccessor(HttpContext httpContext)
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        return mockHttpContextAccessor;
    }

    public static Mock<SignInManager<User>> GetMockSignInManager(WorkoutDbContext db, IHttpContextAccessor httpContextAccessor, Mock<HttpContext> mockHttpContext)
    {
        var userManager = GetUserManager(db);

        var mockSignInManager = new Mock<SignInManager<User>>(
            userManager,
            httpContextAccessor,
            Mock.Of<IUserClaimsPrincipalFactory<User>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<ILogger<SignInManager<User>>>(),
            Mock.Of<IAuthenticationSchemeProvider>(),
            Mock.Of<IUserConfirmation<User>>());

        mockSignInManager.Setup(x => x.SignInAsync(It.IsAny<User>(), It.IsAny<bool>(), It.IsAny<string>()))
            .Callback<User, bool, string>((_user, isPersistent, authenticationMethod) =>
            {
                mockHttpContext.Setup(x => x.User)
                    .Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, _user.Id)
                    })));
            })
            .Returns(Task.CompletedTask);

        mockSignInManager
            .Setup(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        return mockSignInManager;
    }
}