using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.ImpersonationServices;
using Moq;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Services.RoleServices;
using Xunit;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace WorkoutTrackerAPI.Tests.Services;

public class ImpersonationService_Tests : BaseService_Tests
{
    static void SetupMockHttpContext(Mock<HttpContext> mockHttpContext, User user)
    {
        mockHttpContext.Setup(x => x.User)
            .Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            })));
    }

    static Mock<HttpContext> GetMockHttpContext()
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

    static Mock<IHttpContextAccessor> GetMockHttpContextAccessor(HttpContext httpContext)
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
       
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        return mockHttpContextAccessor;
    }

    static Mock<SignInManager<User>> GetMockSignInManager(WorkoutDbContext db, IHttpContextAccessor httpContextAccessor, Mock<HttpContext> mockHttpContext)
    {
        var userManager = IdentityHelper.GetUserManager(db);

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

        return mockSignInManager;
    }

    static IImpersonationService GetImpersonationService(WorkoutDbContext db, Mock<HttpContext> mockHttpContext)
    {
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new ImpersonationService(userRepository, mockSignInManager.Object, mockHttpContextAccessor.Object);
    }


    static async Task<User> GetUserAsync(WorkoutDbContext db)
    {
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        string name = "User";
        string email = "user@email.com";
        string password = "P@$$w0rd";

        var user = await userRepository.GetUserByUsernameAsync(name);

        if (user is null)
        {
            await WorkoutContextFactory.InitializeRolesAsync(db);
            user = await UsersInitializer.InitializeAsync(userRepository, name, email, password, new[] { Roles.UserRole });
        }

        return user;
    }

    static async Task<User> GetImpersonatedUserAsync(WorkoutDbContext db)
    {
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        string name = "ImpersonatedUser";
        string email = "impersonated-user@email.com";
        string password = "P@$$w0rd";

        var user = await userRepository.GetUserByUsernameAsync(name);

        if (user is null)
        {
            await WorkoutContextFactory.InitializeRolesAsync(db);
            user = await UsersInitializer.InitializeAsync(userRepository, name, email, password, new[] { Roles.UserRole });
        }

        return user;
    }


    [Fact]
    public async Task Impersonate_ShouldImpersonate_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var impersonationService = GetImpersonationService(db, mockHttpContext);

        var user = await GetUserAsync(db);
        var impersonatedUser = await GetImpersonatedUserAsync(db);

        SetupMockHttpContext(mockHttpContext, user);

        // Act
        await impersonationService.ImpersonateAsync(impersonatedUser.Id);

        // Assert
        var originalUserId = mockHttpContext.Object.User.FindFirstValue(ClaimTypes.NameIdentifier);
        Assert.NotNull(originalUserId);
        Assert.Equal(impersonatedUser.Id, originalUserId);

        var sessionUserId = mockHttpContext.Object.Session.GetString("OriginalUserId");
        Assert.NotNull(sessionUserId);
        Assert.Equal(user.Id, sessionUserId);
    }

    [Fact]
    public async Task Impersonate_ShouldThrowException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var impersonationService = GetImpersonationService(db, mockHttpContext);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await impersonationService.ImpersonateAsync(null!));
        Assert.Contains("User ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task Impersonate_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var impersonationService = GetImpersonationService(db, mockHttpContext);

        var nonExistenceUserId = Guid.NewGuid().ToString();

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await impersonationService.ImpersonateAsync(nonExistenceUserId));
        Assert.Equal("User to impersonate not found.", ex.Message);
    }

    [Fact]
    public async Task Impersonate_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var impersonationService = new ImpersonationService(userRepository, mockSignInManager.Object, mockHttpContextAccessor.Object);

        var user = await GetUserAsync(db);
        var impersonatedUser = await GetImpersonatedUserAsync(db);

        SetupMockHttpContext(mockHttpContext, user);

        mockSignInManager
            .Setup(repo => repo.SignOutAsync())
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await impersonationService.ImpersonateAsync(impersonatedUser.Id));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task Revert_ShouldRevert_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var impersonationService = GetImpersonationService(db, mockHttpContext);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(mockHttpContext, user);

        var impersonatedUser = await GetImpersonatedUserAsync(db);
        await impersonationService.ImpersonateAsync(impersonatedUser.Id);

        // Act
        await impersonationService.RevertAsync();

        // Assert
        var originalUserId = mockHttpContext.Object!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        Assert.NotNull(originalUserId);
        Assert.Equal(user.Id, originalUserId);

        var sessionUserId = mockHttpContext.Object.Session.GetString("OriginalUserId");
        Assert.Null(sessionUserId);
    }

    [Fact]
    public async Task Revert_ShouldThrowException_WhenOriginalUserIDNotFound()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var impersonationService = GetImpersonationService(db, mockHttpContext);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await impersonationService.RevertAsync());
        Assert.Equal("Original User ID not found.", ex.Message);
    }

    [Fact]
    public async Task Revert_ShouldThrowException_WhenOriginalUserNotFound()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var impersonationService = GetImpersonationService(db, mockHttpContext);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(mockHttpContext, user);

        string notFoundUserID = Guid.NewGuid().ToString();
        mockHttpContext.Object.Session.SetString("OriginalUserId", notFoundUserID);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await impersonationService.RevertAsync());
        Assert.Equal("Original User not found.", ex.Message);
    }

    [Fact]
    public async Task Revert_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var impersonationService = new ImpersonationService(userRepository, mockSignInManager.Object, mockHttpContextAccessor.Object);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(mockHttpContext, user);

        var impersonatedUser = await GetImpersonatedUserAsync(db);
        await impersonationService.ImpersonateAsync(impersonatedUser.Id);

        mockSignInManager
            .Setup(repo => repo.SignOutAsync())
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await impersonationService.RevertAsync());
        Assert.Equal("Database error", ex.Message);
    }



    [Fact]
    public async Task IsImpersonating_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var impersonationService = GetImpersonationService(db, mockHttpContext);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(mockHttpContext, user);

        var impersonatedUser = await GetImpersonatedUserAsync(db);
        await impersonationService.ImpersonateAsync(impersonatedUser.Id);

        // Act
        var result = impersonationService.IsImpersonating();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsImpersonating_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var impersonationService = GetImpersonationService(db, mockHttpContext);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(mockHttpContext, user);

        // Act
        var result = impersonationService.IsImpersonating();

        // Assert
        Assert.False(result);
    }
}
