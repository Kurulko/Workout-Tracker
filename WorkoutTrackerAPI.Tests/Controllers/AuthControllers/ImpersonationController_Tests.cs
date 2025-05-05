using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using WorkoutTracker.API.Controllers.AuthControllers;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories.UserRepositories;
using WorkoutTracker.API.Services.ImpersonationServices;
using Xunit;

namespace WorkoutTracker.API.Tests.Controllers.AuthControllers;

public class ImpersonationController_Tests : APIController_Tests
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

    ImpersonationController GetImpersonationController(WorkoutDbContext db)
    {
        var impersonationService = GetImpersonationService(db);
        return new ImpersonationController(impersonationService);
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
    public async Task ImpersonateAsync_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationController = GetImpersonationController(db);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(user);

        var impersonatedUser = await GetImpersonatedUserAsync(db);

        // Act
        var result = await impersonationController.ImpersonateAsync(impersonatedUser.Id);

        // Assert
        Assert.IsAssignableFrom<OkResult>(result);
    }

    [Fact]
    public async Task ImpersonateAsync_ShouldReturnBadRequest_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationController = GetImpersonationController(db);

        // Act
        var result = await impersonationController.ImpersonateAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task ImpersonateAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var mockImpersonationService = new Mock<ImpersonationService>(userRepository, mockSignInManager.Object, mockHttpContextAccessor.Object);
        var impersonationController = new ImpersonationController(mockImpersonationService.Object);

        var defaultID = Guid.NewGuid().ToString();

        mockImpersonationService
            .Setup(service => service.ImpersonateAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await impersonationController.ImpersonateAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
    }



    [Fact]
    public async Task RevertAsync_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationController = GetImpersonationController(db);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(user);

        var impersonatedUser = await GetImpersonatedUserAsync(db);
        await impersonationController.ImpersonateAsync(impersonatedUser.Id);

        // Act
        var result = await impersonationController.RevertAsync();

        // Assert
        Assert.IsAssignableFrom<OkResult>(result);
    }

    [Fact]
    public async Task RevertAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var mockImpersonationService = new Mock<ImpersonationService>(userRepository, mockSignInManager.Object, mockHttpContextAccessor.Object);
        var impersonationController = new ImpersonationController(mockImpersonationService.Object);

        mockImpersonationService
            .Setup(service => service.RevertAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await impersonationController.RevertAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
    }


    [Fact]
    public async Task IsImpersonating_ShouldReturnTrue_WhenImpersonating()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationController = GetImpersonationController(db);

        var user = await GetUserAsync(db);
        SetupMockHttpContext(user);

        var impersonatedUser = await GetImpersonatedUserAsync(db);
        await impersonationController.ImpersonateAsync(impersonatedUser.Id);

        // Act
        var result = impersonationController.IsImpersonating();

        // Assert
        Assert.True(result.Value);
    }

    [Fact]
    public void IsImpersonating_ShouldReturnFalse_WhenNotImpersonating()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var impersonationController = GetImpersonationController(db);

        // Act
        var result = impersonationController.IsImpersonating();

        // Assert
        Assert.False(result.Value);
    }
}
