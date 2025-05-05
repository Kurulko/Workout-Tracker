using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data.Settings;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Repositories.UserRepositories;
using WorkoutTracker.API.Services.AccountServices;
using Xunit;
using WorkoutTracker.API.Data.Account;
using WorkoutTracker.API.Exceptions;

namespace WorkoutTracker.API.Tests.Services.AuthServices;

public class AccountService_Tests : BaseService_Tests
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
    public async Task RegisterAsync_ShouldRegister_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var accountService = GetAccountService(db);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        // Act
        var authResult = await accountService.RegisterAsync(registerModel);

        // Assert
        Assert.NotNull(authResult);
        Assert.True(authResult.Success);
        Assert.NotNull(authResult.Token);

        var userId = mockHttpContext.Object.User.FindFirstValue(ClaimTypes.NameIdentifier);
        Assert.NotNull(userId);
    }

    [Fact]
    public async Task RegisterAsync_ShouldThrowException_WhenRegisterModelIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var accountService = GetAccountService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EntryNullException>(async () => await accountService.RegisterAsync(null!));
        Assert.Contains("Register entry cannot be null.", ex.Message);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailResult_WhenNameAlreadyRegistered()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var accountService = GetAccountService(db);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        await accountService.RegisterAsync(registerModel);
        registerModel.Email = "new_email@gmail.com";

        // Act
        var authResult = await accountService.RegisterAsync(registerModel);

        // Assert
        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Name already registered.", authResult.Message);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailResult_WhenEmailAlreadyRegistered()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var accountService = GetAccountService(db);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        await accountService.RegisterAsync(registerModel);
        registerModel.Name = "NewName";

        // Act
        var authResult = await accountService.RegisterAsync(registerModel);

        // Assert
        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Email already registered.", authResult.Message);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailResult_WhileCreatingNewUser()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, mockUserRepository.Object, jwtHandler, mockHttpContextAccessor.Object);

        var registerModel = GetValidRegisterModel();
        
        mockUserRepository
            .Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        mockUserRepository
            .Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        mockUserRepository
            .Setup(repo => repo.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to create user"));

        // Act
        var authResult = await accountService.RegisterAsync(registerModel);

        // Assert
        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Register failed: Failed to create user", authResult.Message);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailResult_WhileAddingRolesToUser()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, mockUserRepository.Object, jwtHandler, mockHttpContextAccessor.Object);

        var registerModel = GetValidRegisterModel();
        
        mockUserRepository
            .Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        mockUserRepository
            .Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync((User)null!);

        mockUserRepository
            .Setup(repo => repo.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        mockUserRepository
            .Setup(repo => repo.AddRolesToUserAsync(It.IsAny<string>(), It.IsAny<string[]>()))
            .ThrowsAsync(new Exception("Failed to add roles to user"));

        // Act
        var authResult = await accountService.RegisterAsync(registerModel);

        // Assert
        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Register failed: Failed to add roles to user", authResult.Message);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailResult_WhileGeneratingJwtToken()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var mockJwtHandler = new Mock<JwtHandler>(jwtSettings, userManager);
        var userRepository = new UserRepository(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, userRepository, mockJwtHandler.Object, mockHttpContextAccessor.Object);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        mockJwtHandler
            .Setup(repo => repo.GenerateJwtTokenAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var authResult = await accountService.RegisterAsync(registerModel);

        // Assert
        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Register failed: Database error", authResult.Message);
    }




    [Fact]
    public async Task LoginAsync_ShouldLogin_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var userRepository = new UserRepository(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, userRepository, jwtHandler, mockHttpContextAccessor.Object);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        await accountService.RegisterAsync(registerModel);
        await accountService.LogoutAsync();

        mockSignInManager
            .Setup(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        // Act
        var authResult = await accountService.LoginAsync((LoginModel)registerModel);

        // Assert
        Assert.NotNull(authResult);
        Assert.True(authResult.Success);
        Assert.NotNull(authResult.Token);

        var userId = mockHttpContext.Object.User.FindFirstValue(ClaimTypes.NameIdentifier);
        Assert.NotNull(userId);
    }

    [Fact]
    public async Task LoginAsync_ShouldThrowException_WhenLoginModelIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var accountService = GetAccountService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EntryNullException>(async () => await accountService.LoginAsync(null!));
        Assert.Contains("Login entry cannot be null.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailResult_WhenPasswordOrLoginInvalid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, mockUserRepository.Object, jwtHandler, mockHttpContextAccessor.Object);

        var registerModel = GetValidRegisterModel();

        mockSignInManager
            .Setup(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var authResult = await accountService.LoginAsync((LoginModel)registerModel);

        // Assert
        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Password or/and login invalid", authResult.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailResult_WhileGeneratingJwtToken()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var mockJwtHandler = new Mock<JwtHandler>(jwtSettings, userManager);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, mockUserRepository.Object, mockJwtHandler.Object, mockHttpContextAccessor.Object);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        await accountService.RegisterAsync(registerModel);
        await accountService.LogoutAsync();

        mockJwtHandler
            .Setup(repo => repo.GenerateJwtTokenAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error"));

        mockSignInManager
            .Setup(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Success);

        // Act
        var authResult = await accountService.LoginAsync((LoginModel)registerModel);

        // Assert
        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Login failed: Database error", authResult.Message);
    }


    [Fact]
    public async Task LogoutAsync_ShouldLogout_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, mockUserRepository.Object, jwtHandler, mockHttpContextAccessor.Object);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        await accountService.RegisterAsync(registerModel);

        // Act
        await accountService.LogoutAsync();

        // Assert
        mockSignInManager.Verify(manager => manager.SignOutAsync(), Times.Once());
    }

    [Fact]
    public async Task LogoutAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var userRepository = new UserRepository(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, userRepository, jwtHandler, mockHttpContextAccessor.Object);

        mockSignInManager
            .Setup(repo => repo.SignOutAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await accountService.LogoutAsync());
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task GetTokenAsync_ShouldReturnToken_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var accountService = GetAccountService(db);

        var registerModel = GetValidRegisterModel();
        await WorkoutContextFactory.InitializeRolesAsync(db);

        await accountService.RegisterAsync(registerModel);

        // Act
        var token = await accountService.GetTokenAsync();

        // Assert
        Assert.NotNull(token);
        Assert.NotNull(token.TokenStr);
        Assert.Equal(jwtSettings.ExpirationDays, token.ExpirationDays);
        Assert.Single(token.Roles);
        Assert.Contains(Roles.UserRole, token.Roles);
    }

    [Fact]
    public async Task GetTokenAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = IdentityHelper.GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var mockJwtHandler = new Mock<JwtHandler>(jwtSettings, userManager);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, mockUserRepository.Object, mockJwtHandler.Object, mockHttpContextAccessor.Object);

        mockUserRepository
            .Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()))
            .ReturnsAsync(new User());

        mockJwtHandler
            .Setup(repo => repo.GenerateJwtTokenAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await accountService.GetTokenAsync());
        Assert.Equal("Database error", ex.Message);
    }
}
