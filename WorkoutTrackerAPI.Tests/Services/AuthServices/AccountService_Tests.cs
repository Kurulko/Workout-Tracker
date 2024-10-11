using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Settings;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.AccountServices;
using WorkoutTrackerAPI.Services.ImpersonationServices;
using Xunit;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Services.RoleServices;

namespace WorkoutTrackerAPI.Tests.Services.AuthServices;

public class AccountService_Tests : BaseService_Tests
{
    static readonly JwtSettings jwtSettings = new()
    {
        Audience = "Kurulko's audience",
        Issuer = "KurulkoServer",
        SecretKey = Guid.NewGuid().ToString(),
        ExpirationDays = 5
    };

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


    static IAccountService GetAccountService(WorkoutDbContext db, Mock<HttpContext> mockHttpContext)
    {
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var userRepository = new UserRepository(userManager, db);
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
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var accountService = GetAccountService(db, mockHttpContext);

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
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var accountService = GetAccountService(db, mockHttpContext);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EntryNullException>(async () => await accountService.RegisterAsync(null!));
        Assert.Contains("Register entry cannot be null.", ex.Message);
    }

    [Fact]
    public async Task RegisterAsync_ShouldReturnFailResult_WhenNameAlreadyRegistered()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var accountService = GetAccountService(db, mockHttpContext);

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
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var accountService = GetAccountService(db, mockHttpContext);

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
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
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
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
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
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
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
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
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
        var authResult = await accountService.LoginAsync(ParseToLoginModel(registerModel));

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
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var accountService = GetAccountService(db, mockHttpContext);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EntryNullException>(async () => await accountService.LoginAsync(null!));
        Assert.Contains("Login entry cannot be null.", ex.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailResult_WhenPasswordOrLoginInvalid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var userRepository = new UserRepository(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, userRepository, jwtHandler, mockHttpContextAccessor.Object);

        var registerModel = GetValidRegisterModel();

        mockSignInManager
            .Setup(manager => manager.PasswordSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
            .ReturnsAsync(SignInResult.Failed);

        // Act
        var authResult = await accountService.LoginAsync(ParseToLoginModel(registerModel));

        // Assert
        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Password or/and login invalid", authResult.Message);
    }

    [Fact]
    public async Task LoginAsync_ShouldReturnFailResult_WhileGeneratingJwtToken()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var mockJwtHandler = new Mock<JwtHandler>(jwtSettings, userManager);
        var userRepository = new UserRepository(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, userRepository, mockJwtHandler.Object, mockHttpContextAccessor.Object);

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
        var authResult = await accountService.LoginAsync(ParseToLoginModel(registerModel));

        // Assert
        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Login failed: Database error", authResult.Message);
    }


    [Fact]
    public async Task LogoutAsync_ShouldLogout_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);
        var userRepository = new UserRepository(userManager, db);
        var accountService = new AccountService(mockSignInManager.Object, userRepository, jwtHandler, mockHttpContextAccessor.Object);

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
        using var db = contextFactory.CreateDatabaseContext();
        var mockHttpContext = GetMockHttpContext();
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(db, mockHttpContextAccessor.Object, mockHttpContext);
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


    LoginModel ParseToLoginModel(RegisterModel registerModel)
    {
        var loginModel = new LoginModel
        {
            Name = registerModel.Name,
            Password = registerModel.Password,
            RememberMe = registerModel.RememberMe
        };

        return loginModel;
    }
}
