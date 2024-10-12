using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Controllers.AuthControllers;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.ImpersonationServices;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Settings;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.AccountServices;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using System.Linq.Dynamic.Core.Tokenizer;

namespace WorkoutTrackerAPI.Tests.Controllers.AuthControllers;

public class AccountController_Tests : APIController_Tests
{
    static readonly JwtSettings jwtSettings = new()
    {
        Audience = "Kurulko's audience",
        Issuer = "KurulkoServer",
        SecretKey = Guid.NewGuid().ToString(),
        ExpirationDays = 5
    };

    readonly Mock<HttpContext> mockHttpContext = IdentityHelper.GetMockHttpContext();

    IAccountService GetAccountService(WorkoutDbContext db)
    {
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        return new AccountService(mockSignInManager.Object, userRepository, jwtHandler, mockHttpContextAccessor.Object);
    }

    AccountController GetAccountController(WorkoutDbContext db)
    {
        var accountService = GetAccountService(db);
        return new AccountController(accountService);
    }

    static RegisterModel GetValidRegisterModel()
    {
        return new()
        {
            Name = "Kurulko",
            Email = "kurulko@gmail.com",
            Password = "P@$$w0rd",
            PasswordConfirm = "P@$$w0rd",
        };
    }


    [Fact]
    public async Task RegisterAsync_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var accountController = GetAccountController(db);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        // Act
        var result = await accountController.RegisterAsync(registerModel);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsAssignableFrom<AuthResult>(okResult.Value);

        var authResult = (okResult.Value as AuthResult)!;
        Assert.True(authResult.Success);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnBadRequest_WhenRegisterModelIsNull()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var accountController = GetAccountController(db);

        // Act
        var result = await accountController.RegisterAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Register entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnBadRequest_WhenRegisterModelIsInvalid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var accountController = GetAccountController(db);

        var registerModel = new RegisterModel();

        accountController.ModelState.AddModelError("Name", "Required");
        accountController.ModelState.AddModelError("Password", "Minimum length 8 required");

        // Act
        var result = await accountController.RegisterAsync(registerModel);

        // Assert
        var okResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }


    [Fact]
    public async Task RegisterAsync_ShouldReturnBadRequest_WhenUnauthorized()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var mockAccountService = new Mock<AccountService>(mockSignInManager.Object, userRepository, jwtHandler, mockHttpContextAccessor.Object);
        var accountController = new AccountController(mockAccountService.Object);

        var registerModel = GetValidRegisterModel();

        mockAccountService
            .Setup(service => service.RegisterAsync(It.IsAny<RegisterModel>()))
            .ReturnsAsync(AuthResult.Fail("Access denied"));

        // Act
        var result = await accountController.RegisterAsync(registerModel);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
        Assert.IsAssignableFrom<AuthResult>(unauthorizedResult.Value);

        var authResult = (unauthorizedResult.Value as AuthResult)!;
        Assert.False(authResult.Success);
        Assert.Equal("Access denied", authResult.Message);
    }



    [Fact]
    public async Task LoginAsync_ShouldReturnOk_WhenValidRequest()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var accountController = GetAccountController(db);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        await accountController.RegisterAsync(registerModel);
        await accountController.LogoutAsync();

        // Act
        var result = await accountController.LoginAsync((LoginModel)registerModel);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
        Assert.IsAssignableFrom<AuthResult>(okResult.Value);

        var authResult = (okResult.Value as AuthResult)!;
        Assert.True(authResult.Success);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnBadRequest_WhenLoginModelIsNull()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var accountController = GetAccountController(db);

        // Act
        var result = await accountController.LoginAsync(null!);

        // Assert
        var okResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Login entry is null.", okResult.Value);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnBadRequest_WhenLoginModelIsInvalid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var accountController = GetAccountController(db);

        var registerModel = new RegisterModel();

        accountController.ModelState.AddModelError("Name", "Required");
        accountController.ModelState.AddModelError("Password", "Minimum length 8 required");

        // Act
        var result = await accountController.LoginAsync((LoginModel)registerModel);

        // Assert
        var okResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }


    [Fact]
    public async Task LoginAsync_ShouldReturnBadRequest_WhenUnauthorized()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var mockAccountService = new Mock<AccountService>(mockSignInManager.Object, userRepository, jwtHandler, mockHttpContextAccessor.Object);
        var accountController = new AccountController(mockAccountService.Object);

        var registerModel = GetValidRegisterModel();

        mockAccountService
            .Setup(service => service.LoginAsync(It.IsAny<LoginModel>()))
            .ReturnsAsync(AuthResult.Fail("Access denied"));

        // Act
        var result = await accountController.LoginAsync((LoginModel)registerModel);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.NotNull(unauthorizedResult.Value);
        Assert.IsAssignableFrom<AuthResult>(unauthorizedResult.Value);

        var authResult = (unauthorizedResult.Value as AuthResult)!;
        Assert.False(authResult.Success);
        Assert.Equal("Access denied", authResult.Message);
    }



    [Fact]
    public async Task GetTokenAsync_ShouldReturnOkToken_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var accountController = GetAccountController(db);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        await accountController.RegisterAsync(registerModel);

        // Act
        var result = await accountController.GetTokenAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsAssignableFrom<TokenModel>(okResult.Value);

        var token = (okResult.Value as TokenModel)!;

        Assert.NotNull(token);
        Assert.NotNull(token.TokenStr);
        Assert.Equal(jwtSettings.ExpirationDays, token.ExpirationDays);
        Assert.Single(token.Roles);
        Assert.Contains(Roles.UserRole, token.Roles);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var mockAccountService = new Mock<AccountService>(mockSignInManager.Object, userRepository, jwtHandler, mockHttpContextAccessor.Object);
        var accountController = new AccountController(mockAccountService.Object);

        mockAccountService
            .Setup(repo => repo.GetTokenAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await accountController.GetTokenAsync();

        //Assert
        var objecttResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal("Internal server error: Database error", objecttResult.Value);
        Assert.Equal(500, objecttResult.StatusCode);
    }
}
