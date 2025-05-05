using WorkoutTracker.API.Data;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Services.ImpersonationServices;
using Moq;
using WorkoutTracker.API.Exceptions;
using WorkoutTracker.API.Services.RoleServices;
using Xunit;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories.UserRepositories;
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

namespace WorkoutTracker.API.Tests.Services;

public class ImpersonationService_Tests : BaseService_Tests
{
    readonly Mock<HttpContext> mockHttpContext = IdentityHelper.GetMockHttpContext();

    void SetupMockHttpContext(User user)
    {
        mockHttpContext.Setup(x => x.User)
            .Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            })));
    }

    IImpersonationService GetImpersonationService(WorkoutDbContext db)
    {
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
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
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationService = GetImpersonationService(db);

        var user = await GetUserAsync(db);
        var impersonatedUser = await GetImpersonatedUserAsync(db);

        SetupMockHttpContext(user);

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
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationService = GetImpersonationService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await impersonationService.ImpersonateAsync(null!));
        Assert.Contains("User ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task Impersonate_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationService = GetImpersonationService(db);

        var nonExistenceUserId = Guid.NewGuid().ToString();

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await impersonationService.ImpersonateAsync(nonExistenceUserId));
        Assert.Equal("User to impersonate not found.", ex.Message);
    }

    [Fact]
    public async Task Impersonate_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var mockImpersonationService = new Mock<ImpersonationService>(userRepository, mockSignInManager.Object, mockHttpContextAccessor.Object);
        var impersonationService = new ImpersonationService(userRepository, mockSignInManager.Object, mockHttpContextAccessor.Object);

        var user = await GetUserAsync(db);
        var impersonatedUser = await GetImpersonatedUserAsync(db);

        SetupMockHttpContext(user);

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
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationService = GetImpersonationService(db);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(user);

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
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationService = GetImpersonationService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await impersonationService.RevertAsync());
        Assert.Equal("Original User ID not found.", ex.Message);
    }

    [Fact]
    public async Task Revert_ShouldThrowException_WhenOriginalUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationService = GetImpersonationService(db);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(user);

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
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var mockImpersonationService = new Mock<ImpersonationService>(userRepository, mockSignInManager.Object, mockHttpContextAccessor.Object);
        var impersonationService = new ImpersonationService(userRepository, mockSignInManager.Object, mockHttpContextAccessor.Object);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(user);

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
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationService = GetImpersonationService(db);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(user);

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
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationService = GetImpersonationService(db);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(user);

        // Act
        var result = impersonationService.IsImpersonating();

        // Assert
        Assert.False(result);
    }
}
