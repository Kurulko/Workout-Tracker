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

namespace WorkoutTrackerAPI.Tests.Services;

public class MockHttpSession : ISession
{
    Dictionary<string, object> sessionStorage = new Dictionary<string, object>();

    public object this[string name]
    {
        get { return sessionStorage[name]; }
        set { sessionStorage[name] = value; }
    }

    public string Id
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public bool IsAvailable
    {
        get
        {
            throw new NotImplementedException();
        }
    }

    public IEnumerable<string> Keys
    {
        get { return sessionStorage.Keys; }
    }

    public void Clear()
    {
        sessionStorage.Clear();
    }

    public Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
    }

    public Task LoadAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        throw new NotImplementedException();
    }

    public void Remove(string key)
    {
        sessionStorage.Remove(key);
    }

    public void Set(string key, byte[] value)
    {
        sessionStorage[key] = value;
    }

    public bool TryGetValue(string key, out byte[] value)
    {
        if (sessionStorage[key] != null)
        {
            value = Encoding.ASCII.GetBytes(sessionStorage[key].ToString());
            return true;
        }
        else
        {
            value = null;
            return false;
        }
    }
}

public class ImpersonationService_Tests : BaseService_Tests
{
    static Mock<HttpContext> GetMockHttpContext(User user)
    {
        var mockHttpContext = new Mock<HttpContext>();
        var mockSession = new MockHttpSession();
        //var mockSession = new Mock<ISession>();

        //var sessionStorage = new Dictionary<string, string>();

        //mockSession.Setup(x => x.GetString(It.IsAny<string>()))
        //    .Returns((string key) => sessionStorage.TryGetValue(key, out var value) ? value : null);

        //mockSession.Setup(x => x.SetString(It.IsAny<string>(), It.IsAny<string>()))
        //    .Callback<string, string>((key, value) => sessionStorage[key] = value);

        mockHttpContext.Setup(x => x.User)
            .Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            })));

        mockHttpContext.Setup(x => x.Session).Returns(mockSession);
        //mockHttpContext.Setup(x => x.Session).Returns(mockSession.Object);

        return mockHttpContext;
    }

    static Mock<IHttpContextAccessor> GetMockHttpContextAccessor(HttpContext httpContext)
    {
        var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
       
        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        return mockHttpContextAccessor;
    }

    static Mock<SignInManager<User>> GetMockSignInManager(UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, Mock<HttpContext> mockHttpContext)
    {
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

        var user = await GetUserAsync(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var mockHttpContext = GetMockHttpContext(user);
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(userManager, mockHttpContextAccessor.Object, mockHttpContext);
        var impersonationService = new ImpersonationService(userManager, mockSignInManager.Object, mockHttpContextAccessor.Object);

        var impersonatedUser = await GetImpersonatedUserAsync(db);

        // Act
        await impersonationService.ImpersonateAsync(impersonatedUser.Id);

        // Assert
        var originalUserId = mockHttpContext.Object.User.FindFirstValue(ClaimTypes.NameIdentifier);
        Assert.NotNull(originalUserId);
        Assert.Equal(impersonatedUser.Id, originalUserId);

        var originalUserId2 = mockHttpContext.Object.Session.GetString("OriginalUserId");
        Assert.NotNull(originalUserId2);
        Assert.Equal(impersonatedUser.Id, originalUserId2);
    }

    [Fact]
    public async Task Impersonate_ShouldThrowException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>();
        var signInManager = IdentityHelper.GetSignInManager(db, httpContextAccessor);
        var impersonationService = new ImpersonationService(userManager, signInManager, httpContextAccessor);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await impersonationService.ImpersonateAsync(null!));
        Assert.Equal("User ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task Impersonate_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>();
        var signInManager = IdentityHelper.GetSignInManager(db, httpContextAccessor);
        var impersonationService = new ImpersonationService(userManager, signInManager, httpContextAccessor);

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

        var user = await GetUserAsync(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var mockHttpContext = GetMockHttpContext(user);
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(userManager, mockHttpContextAccessor.Object, mockHttpContext);

        var impersonationService = new ImpersonationService(userManager, mockSignInManager.Object, mockHttpContextAccessor.Object);

        mockSignInManager
            .Setup(repo => repo.SignOutAsync())
            .ThrowsAsync(new Exception("Database error"));

        var impersonatedUser = await GetImpersonatedUserAsync(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await impersonationService.ImpersonateAsync(impersonatedUser.Id));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task Revert_ShouldRevert_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var user = await GetUserAsync(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var mockHttpContext = GetMockHttpContext(user);
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(userManager, mockHttpContextAccessor.Object, mockHttpContext);

        var impersonationService = new ImpersonationService(userManager, mockSignInManager.Object, mockHttpContextAccessor.Object);

        var impersonatedUser = await GetImpersonatedUserAsync(db);
        await impersonationService.ImpersonateAsync(impersonatedUser.Id);

        // Act
        await impersonationService.RevertAsync();

        // Assert
        var originalUserId = mockHttpContextAccessor.Object.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier);
        Assert.NotNull(originalUserId);
        Assert.Equal(user.Id, originalUserId);
    }

    [Fact]
    public async Task Revert_ShouldThrowException_WhenOriginalUserIDNotFound()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var httpContextAccessor = Mock.Of<IHttpContextAccessor>();
        var signInManager = IdentityHelper.GetSignInManager(db, httpContextAccessor);
        var impersonationService = new ImpersonationService(userManager, signInManager, httpContextAccessor);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await impersonationService.RevertAsync());
        Assert.Equal("Original User ID not found.", ex.Message);
    }

    [Fact]
    public async Task Revert_ShouldThrowException_WhenOriginalUserNotFound()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var user = await GetUserAsync(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var mockHttpContext = GetMockHttpContext(user);
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(userManager, mockHttpContextAccessor.Object, mockHttpContext);

        var impersonationService = new ImpersonationService(userManager, mockSignInManager.Object, mockHttpContextAccessor.Object);

        mockHttpContext
            .Setup(x => x.Session.GetString(It.IsAny<string>()))
            .Returns(() => Guid.NewGuid().ToString());

        var impersonatedUser = await GetImpersonatedUserAsync(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await impersonationService.RevertAsync());
        Assert.Equal("Original User not found.", ex.Message);
    }

    [Fact]
    public async Task Revert_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var user = await GetUserAsync(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var mockHttpContext = GetMockHttpContext(user);
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(userManager, mockHttpContextAccessor.Object, mockHttpContext);

        var impersonationService = new ImpersonationService(userManager, mockSignInManager.Object, mockHttpContextAccessor.Object);

        mockSignInManager
            .Setup(repo => repo.SignOutAsync())
            .ThrowsAsync(new Exception("Database error"));

        var impersonatedUser = await GetImpersonatedUserAsync(db);
        await impersonationService.ImpersonateAsync(impersonatedUser.Id);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await impersonationService.RevertAsync());
        Assert.Equal("Database error", ex.Message);
    }



    [Fact]
    public async Task IsImpersonating_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var user = await GetUserAsync(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var mockHttpContext = GetMockHttpContext(user);
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(userManager, mockHttpContextAccessor.Object, mockHttpContext);

        var impersonationService = new ImpersonationService(userManager, mockSignInManager.Object, mockHttpContextAccessor.Object);

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

        var user = await GetUserAsync(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var mockHttpContext = GetMockHttpContext(user);
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(userManager, mockHttpContextAccessor.Object, mockHttpContext);

        var impersonationService = new ImpersonationService(userManager, mockSignInManager.Object, mockHttpContextAccessor.Object);

        // Act
        var result = impersonationService.IsImpersonating();

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task IsImpersonating_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var user = await GetUserAsync(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var mockHttpContext = GetMockHttpContext(user);
        var mockHttpContextAccessor = GetMockHttpContextAccessor(mockHttpContext.Object);
        var mockSignInManager = GetMockSignInManager(userManager, mockHttpContextAccessor.Object, mockHttpContext);

        var impersonationService = new ImpersonationService(userManager, mockSignInManager.Object, mockHttpContextAccessor.Object);

        //mockHttpContext
        //    .Setup(x => x.Session.GetString(It.IsAny<string>()))
        //    .Throws(new Exception("Database error"));

        mockHttpContext
            .Setup(x => x.Session)
            .Throws(new Exception("Database error"));

        //Act & Assert
        var ex = Assert.Throws<Exception>(() => impersonationService.IsImpersonating());
        Assert.Equal("Database error", ex.Message);
    }
}
