using Moq;
using System.Security.Claims;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data.Models.UserModels;
using WorkoutTracker.API.Exceptions;
using WorkoutTracker.API.Extentions;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Repositories.UserRepositories;
using WorkoutTracker.API.Services;
using WorkoutTracker.API.Services.ExerciseRecordServices;
using WorkoutTracker.API.Services.RoleServices;
using WorkoutTracker.API.Services.UserServices;
using Xunit;

namespace WorkoutTracker.API.Tests.Services.UserServices;

public class UserService_Tests : BaseService_Tests
{
    static User CreateUser(string name, string email)
    {
        User user = new()
        {
            UserName = name,
            Email = email,
            Registered = DateTime.Now
        };

        return user;
    }

    static User GetValidUser()
    {
        return CreateUser("User", "user@gmail.com");
    }

    static IEnumerable<User> GetValidUsers()
    {
        var user_User = CreateUser("User", "user@gmail.com");
        var admin_User = CreateUser("Admin", "admin@gmail.com");

        return new[] { user_User, admin_User }; ;
    }


    static RoleRepository GetRoleRepository(WorkoutDbContext db)
    {
        var roleManager = IdentityHelper.GetRoleManager(db);
        return new RoleRepository(roleManager);
    }

    static UserRepository GetUserRepository(WorkoutDbContext db)
    {
        var userManager = IdentityHelper.GetUserManager(db);
        return new UserRepository(userManager, db);
    }

    static IUserService GetUserService(WorkoutDbContext db)
    {
        var userRepository = GetUserRepository(db);
        var roleRepository = GetRoleRepository(db);
        return new UserService(userRepository, roleRepository);
    }


    #region CRUD

    [Fact]
    public async Task AddUserAsync_ShouldThrowEntryNullException_WhenUserIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EntryNullException>(() => userService.AddUserAsync(null!));
        Assert.Equal("User", ex.ParamName);
    }

    [Fact]
    public async Task AddUserAsync_ShouldThrowException_WhenUserAlreadyExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.AddUserAsync(user));
        Assert.Equal("User already exists.", ex.Message);
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnUser_WhenSuccessfullyAdded()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var user = GetValidUser();

        // Act
        var result = await userService.AddUserAsync(user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task AddUserAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();

        mockUserRepository
            .Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error."));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await userService.AddUserAsync(user));
        Assert.Equal("Database error.", ex.Message);
    }



    [Fact]
    public async Task CreateUserAsync_ShouldReturnFailedResult_WhenUserIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string password = "P@$$w0rd";

        // Act
        var result = await userService.CreateUserAsync(null!, password);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User entry cannot be null."));
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnFailedResult_WhenPasswordIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var user = GetValidUser();

        // Act
        var result = await userService.CreateUserAsync(user, null!);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("Password cannot be null or empty.");
        Assert.True(errorExists);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnSuccessResult_WhenUserIsCreatedSuccessfully()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        string password = "P@$$w0rd";

        // Act
        var result = await userService.CreateUserAsync(user, password);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task CreateUserAsync__ShouldReturnFailedResult_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        string password = "P@$$w0rd";

        mockUserRepository
            .Setup(repo => repo.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act
        var result = await userService.CreateUserAsync(user, password);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("Database error.");
        Assert.True(errorExists);
    }



    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFailedResult_WhenUserIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act
        var result = await userService.UpdateUserAsync(null!);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("User entry cannot be null.");
        Assert.True(errorExists);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFailedResult_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        user.Id = string.Empty;

        // Act
        var result = await userService.UpdateUserAsync(user);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("User ID cannot be null or empty.");
        Assert.True(errorExists);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFailedResult_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        user.Id = "non-existent-userId";

        // Act
        var result = await userService.UpdateUserAsync(user);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User not found."));
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnSuccessResult_WhenUserIsUpdatedSuccessfully()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.UpdateUserAsync(user);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnFailedResult_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.UpdateUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act
        var result = await userService.UpdateUserAsync(user);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("Database error.");
        Assert.True(errorExists);
    }



    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailedResult_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act
        var result = await userService.DeleteUserAsync(null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User ID cannot be null or empty."));
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailedResult_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserId = "non-existent-userId";

        // Act
        var result = await userService.DeleteUserAsync(nonExistentUserId);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User not found."));
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnSuccessResult_WhenUserIsDeletedSuccessfully()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.DeleteUserAsync(user.Id);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnFailedResult_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.DeleteUserAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act
        var result = await userService.DeleteUserAsync(user.Id);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("Database error."));
    }



    [Fact]
    public async Task GetUserByClaimsAsync_ShouldThrowEntryNullException_WhenClaimsAreNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<EntryNullException>(() => userService.GetUserByClaimsAsync(null!));
        Assert.Equal("Claims", ex.ParamName);
    }

    [Fact]
    public async Task GetUserByClaimsAsync_ShouldReturnUser_WhenClaimsAreValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.UserName) }));

        // Act
        var result = await userService.GetUserByClaimsAsync(claimsPrincipal);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetUserByClaimsAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserName = "nonExistentUserName";
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, nonExistentUserName) }));

        // Act
        var result = await userService.GetUserByClaimsAsync(claimsPrincipal);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByClaimsAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, user.UserName) }));

        mockUserRepository
            .Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.GetUserByClaimsAsync(claimsPrincipal));
        Assert.Equal("Database error.", ex.Message);
    }



    [Fact]
    public async Task GetUserIdByUsernameAsync_ShouldThrowArgumentException_WhenUserNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserIdByUsernameAsync(null!));
        Assert.Equal("User name", ex.ParamName);
    }

    [Fact]
    public async Task GetUserIdByUsernameAsync_ShouldReturnUserId_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.GetUserIdByUsernameAsync(user.UserName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result);
    }

    [Fact]
    public async Task GetUserIdByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserName = "nonExistentUserName";

        // Act
        var result = await userService.GetUserIdByUsernameAsync(nonExistentUserName);

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetUserIdByUsernameAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.GetUserIdByUsernameAsync(user.UserName));
        Assert.Equal("Database error.", ex.Message);
    }

    [Fact]
    public async Task GetUserNameByIdAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserNameByIdAsync(null!));
        Assert.Equal("User ID", ex.ParamName);
    }

    [Fact]
    public async Task GetUserNameByIdAsync_ShouldReturnUserName_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.GetUserNameByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.UserName, result);
    }

    [Fact]
    public async Task GetUserNameByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "nonExistentUserID";

        // Act
        var result = await userService.GetUserNameByIdAsync(nonExistentUserID);

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetUserNameByIdAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.GetUserByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.GetUserNameByIdAsync(user.Id));
        Assert.Equal("Database error.", ex.Message);
    }



    [Fact]
    public async Task GetUserByUsernameAsync_ShouldThrowArgumentException_WhenUserNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserByUsernameAsync(null!));
        Assert.Equal("User name", ex.ParamName);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.GetUserByUsernameAsync(user.UserName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserName = "nonExistentUserName";

        // Act
        var result = await userService.GetUserByUsernameAsync(nonExistentUserName);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.GetUserByUsernameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.GetUserByUsernameAsync(user.UserName));
        Assert.Equal("Database error.", ex.Message);
    }



    [Fact]
    public async Task GetUsersAsync_ShouldReturnAllUsers()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var users = GetValidUsers();
        foreach (var user in users)
        {
            await userService.AddUserAsync(user);
        }

        // Act
        var result = await userService.GetUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(users.Count(), result.Count());
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act
        var result = await userService.GetUsersAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldThrowException()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        mockUserRepository
            .Setup(repo => repo.GetUsersAsync())
            .ThrowsAsync(new Exception("Database error."));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.GetUsersAsync());
        Assert.Equal("Database error.", ex.Message);
    }


    [Fact]
    public async Task GetUserByIdAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserByIdAsync(null!));
        Assert.Equal("User ID", ex.ParamName);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.GetUserByIdAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user, result);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";

        // Act
        var result = await userService.GetUserByUsernameAsync(nonExistentUserID);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.GetUserByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.GetUserByIdAsync(user.Id));
        Assert.Equal("Database error.", ex.Message);
    }



    [Fact]
    public async Task UserExistsAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.UserExistsAsync(null!));
        Assert.Equal("User ID", ex.ParamName);
    }

    [Fact]
    public async Task UserExistsAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.UserExistsAsync(user.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserExistsAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";

        // Act
        var result = await userService.UserExistsAsync(nonExistentUserID);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserExistsAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.UserExistsAsync(user.Id));
        Assert.Equal("Database error.", ex.Message);
    }


    [Fact]
    public async Task UserExistsByUsernameAsync_ShouldThrowArgumentException_WhenUserNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.UserExistsByUsernameAsync(null!));
        Assert.Equal("User name", ex.ParamName);
    }

    [Fact]
    public async Task UserExistsByUsernameAsync_ShouldReturnTrue_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.UserExistsByUsernameAsync(user.UserName);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserExistsByUsernameAsync_ShouldReturnFalse_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserName = "non-existent-userName";

        // Act
        var result = await userService.UserExistsByUsernameAsync(nonExistentUserName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserExistsByUsernameAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsByUsernameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.UserExistsByUsernameAsync(user.Id));
        Assert.Equal("Database error.", ex.Message);
    }


    #endregion

    #region User Models

    [Fact]
    public async Task GetUserExerciseRecordsAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserExerciseRecordsAsync(null!));
        Assert.Equal("User ID", ex.ParamName);
    }

    [Fact]
    public async Task GetUserExerciseRecordsAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => userService.GetUserExerciseRecordsAsync(nonExistentUserID));
        Assert.Equal("User", ex.ParamName);
    }

    [Fact]
    public async Task GetUserExerciseRecordsAsync_ShouldReturnExerciseRecords_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var exerciseRepository = new ExerciseRepository(db);
        var muscleRepository = new MuscleRepository(db);
        var exercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Pull Up", ExerciseType.Reps, "Latissimus dorsi", "Biceps brachii", "Teres minor");

        var exerciseRecords = new[]
             {
                new ExerciseRecord()
                {
                    Date = DateTime.Now,
                    Reps = 20,
                    SumOfReps = 39,
                    CountOfTimes = 2,
                    ExerciseId = exercise.Id
                },
                new ExerciseRecord()
                {
                    Date = DateTime.Now.AddDays(-2),
                    Reps = 19,
                    SumOfReps = 19,
                    CountOfTimes = 1,
                    ExerciseId = exercise.Id
                }
            };

        var userRepository = GetUserRepository(db);
        var exerciseRecordRepository = new ExerciseRecordRepository(db);
        var exerciseRecordService = new ExerciseRecordService(exerciseRecordRepository, userRepository);

        foreach (var exerciseRecord in exerciseRecords)
        {
            await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);
        }

        // Act
        var result = await userService.GetUserExerciseRecordsAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(exerciseRecords.Length, result.Count());
    }
    
    [Fact]
    public async Task GetUserExerciseRecordsAsync_ShouldReturnEmpty_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.GetUserExerciseRecordsAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserExerciseRecordsAsyn_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.GetUserExerciseRecordsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await userService.GetUserExerciseRecordsAsync(user.Id));
        Assert.Equal("Database error.", ex.Message);
    }



    [Fact]
    public async Task GetUserMuscleSizesAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserMuscleSizesAsync(null!));
        Assert.Equal("User ID", ex.ParamName);
    }

    [Fact]
    public async Task GetUserMuscleSizesAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => userService.GetUserMuscleSizesAsync(nonExistentUserID));
        Assert.Equal("User", ex.ParamName);
    }

    [Fact]
    public async Task GetUserMuscleSizesAsync_ShouldReturnMuscleSizes_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        var muscleRepository = new MuscleRepository(db);
        var muscle = await MusclesInitializer.InitializeAsync(muscleRepository, new() { Name = "Back" }, null);

        var muscleSizes = new[]
        {
                new MuscleSize()
                {
                    Date = DateTime.Now.AddDays(-100),
                    Size = 35,
                    SizeType = SizeType.Centimeter,
                    MuscleId = muscle.Id,
                },
                new MuscleSize()
                {
                    Date = DateTime.Now.AddDays(-30),
                    Size = 40,
                    SizeType = SizeType.Centimeter,
                    MuscleId = muscle.Id,
                },
                new MuscleSize()
                {
                    Date = DateTime.Now,
                    Size = 42,
                    SizeType = SizeType.Centimeter,
                    MuscleId = muscle.Id,
                }
        };

        var muscleSizeRepository = new MuscleSizeRepository(db);
        var userRepository = GetUserRepository(db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepository, userRepository);

        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        // Act
        var result = await userService.GetUserMuscleSizesAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(muscleSizes.Count(), result.Count());
    }

    [Fact]
    public async Task GetUserMuscleSizesAsync_ShouldReturnEmpty_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.GetUserMuscleSizesAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserMuscleSizesAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.GetUserMuscleSizesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await userService.GetUserMuscleSizesAsync(user.Id));
        Assert.Equal("Database error.", ex.Message);
    }



    [Fact]
    public async Task GetUserBodyWeightsAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserBodyWeightsAsync(null!));
        Assert.Equal("User ID", ex.ParamName);
    }

    [Fact]
    public async Task GetUserBodyWeightsAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => userService.GetUserBodyWeightsAsync(nonExistentUserID));
        Assert.Equal("User", ex.ParamName);
    }

    [Fact]
    public async Task GetUserBodyWeightsAsync_ShouldReturnBodyWeights_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        var bodyWeights = new[]
           {
                new BodyWeight()
                {
                    Date = DateTime.Now,
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                },
                new BodyWeight()
                {
                    Date = DateTime.Now.AddDays(-2),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                }
            };

        var bodyWeightRepository = new BodyWeightRepository(db);
        var userRepository = GetUserRepository(db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepository, userRepository);

        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        // Act
        var result = await userService.GetUserBodyWeightsAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(bodyWeights.Count(), result.Count());
    }

    [Fact]
    public async Task GetUserBodyWeightsAsync_ShouldReturnEmpty_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.GetUserBodyWeightsAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserBodyWeightsAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.GetUserBodyWeightsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await userService.GetUserBodyWeightsAsync(user.Id));
        Assert.Equal("Database error.", ex.Message);
    }



    [Fact]
    public async Task GetUserWorkoutsAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserWorkoutsAsync(null!));
        Assert.Equal("User ID", ex.ParamName);
    }

    [Fact]
    public async Task GetUserWorkoutsAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => userService.GetUserBodyWeightsAsync(nonExistentUserID));
        Assert.Equal("User", ex.ParamName);
    }

    [Fact]
    public async Task GetUserWorkoutsAsync_ShouldReturnWorkouts_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        var muscleRepository = new MuscleRepository(db);
        var exerciseRepository = new ExerciseRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var plankExercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
        var standingCalfRaisesExercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Standing Calf Raises", ExerciseType.WeightAndReps, "Gastrocnemius", "Soleus");
        var legRaiseExercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Leg Raise", ExerciseType.Reps, "Rectus abdominis", "Hip flexors");

        var workouts = new[] {
            new Workout()
            {
                Name = "Abs",
                Exercises = new[] { plankExercise },
            },
            new Workout()
            {
                Name = "Abs and legs",
                Exercises = new[] { plankExercise, legRaiseExercise },
            },
            new Workout()
            {
                Name = "Legs",
                Exercises = new[] { standingCalfRaisesExercise, legRaiseExercise },
            }
        };

        var workoutRepository = new WorkoutRepository(db);
        var userRepository = GetUserRepository(db);
        var workoutService = new WorkoutService(workoutRepository, userRepository);

        foreach (var workout in workouts)
        {
            await workoutService.AddUserWorkoutAsync(user.Id, workout);
        }

        // Act
        var result = await userService.GetUserWorkoutsAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(workouts.Count(), result.Count());
    }

    [Fact]
    public async Task GetUserWorkoutsAsync_ShouldReturnEmpty_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.GetUserWorkoutsAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserWorkoutsAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.GetUserWorkoutsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await userService.GetUserWorkoutsAsync(user.Id));
        Assert.Equal("Database error.", ex.Message);
    }



    [Fact]
    public async Task GetUserCreatedExercisesAsync_ShouldThrowArgumentException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserCreatedExercisesAsync(null!));
        Assert.Equal("User ID", ex.ParamName);
    }

    [Fact]
    public async Task GetUserCreatedExercisesAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => userService.GetUserCreatedExercisesAsync(nonExistentUserID));
        Assert.Equal("User", ex.ParamName);
    }

    [Fact]
    public async Task GetUserCreatedExercisesAsync_ShouldReturnExercises_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var muscleRepository = new MuscleRepository(db);

        async Task<Exercise> CreateExerciseAsync(string name, ExerciseType exerciseType, params string[] muscleNames)
        {
            Exercise exercise = new();

            exercise.Name = name;
            exercise.Type = exerciseType;

            var muscles = new List<Muscle>();

            foreach (string muscleName in muscleNames)
            {
                var muscle = await muscleRepository!.GetByNameAsync(muscleName);
                if (muscle is not null)
                    muscles.Add(muscle);
            }

            exercise.WorkingMuscles = muscles;
            exercise.CreatedByUserId = user!.Id;

            return exercise;
        }

        var plankExercise = await CreateExerciseAsync("Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
        var pullUpExercise = await CreateExerciseAsync("Pull Up", ExerciseType.Reps, "Latissimus dorsi", "Biceps brachii", "Teres minor");
        var pushUpExercise = await CreateExerciseAsync("Push Up", ExerciseType.Reps, "Pectoralis major", "Triceps brachii", "Deltoids");
        var exercises = new[] { plankExercise, pullUpExercise, pushUpExercise };

        var exerciseRepository = new ExerciseRepository(db);
        var userRepository = GetUserRepository(db);
        var exerciseService = new ExerciseService(exerciseRepository, userRepository);

        foreach (var exercise in exercises)
        {
            await exerciseService.AddUserExerciseAsync(user.Id, exercise);
        }

        // Act
        var result = await userService.GetUserCreatedExercisesAsync(user.Id);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetUserCreatedExercisesAsync_ShouldReturnEmpty_WhenUserExists()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.GetUserCreatedExercisesAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserCreatedExercisesAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.GetUserCreatedExercisesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error."));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await userService.GetUserCreatedExercisesAsync(user.Id));
        Assert.Equal("Database error.", ex.Message);
    }


    #endregion

    #region Password

    [Fact]
    public async Task ChangeUserPasswordAsync_ShouldReturnFailedResult_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string oldPassword = "old-P@$$w0rd";
        string newPassword = "new-P@$$w0rd";

        // Act
        var result = await userService.ChangeUserPasswordAsync(null!, oldPassword, newPassword);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User ID cannot be null or empty."));
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ShouldReturnFailedResult_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";
        string oldPassword = "old-P@$$w0rd";
        string newPassword = "new-P@$$w0rd";

        // Act
        var result = await userService.ChangeUserPasswordAsync(nonExistentUserID, oldPassword, newPassword);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User not found."));
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ShouldReturnFailedResult_WhenOldPasswordOrNewPasswordIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        string oldPassword = "old-P@$$w0rd";
        string newPassword = "new-P@$$w0rd";

        // Act
        var resultEmptyOld = await userService.ChangeUserPasswordAsync(user.Id, null!, newPassword);
        var resultEmptyNew = await userService.ChangeUserPasswordAsync(user.Id, oldPassword, null!);
        var resultEmptyBoth = await userService.ChangeUserPasswordAsync(user.Id, null!, null!);

        // Assert
        Assert.False(resultEmptyOld.Succeeded);
        Assert.True(resultEmptyOld.ErrorExists("Old or new password cannot be null or empty."));

        Assert.False(resultEmptyNew.Succeeded);
        Assert.True(resultEmptyNew.ErrorExists("Old or new password cannot be null or empty."));

        Assert.False(resultEmptyBoth.Succeeded);
        Assert.True(resultEmptyBoth.ErrorExists("Old or new password cannot be null or empty."));
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ShouldReturnFailedResult_WhenOldPasswordEqualsNewPassword()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);


        var user = GetValidUser();
        await userService.AddUserAsync(user);

        string oldPassword = "P@$$w0rd";
        string newPassword = oldPassword;

        // Act
        var result = await userService.ChangeUserPasswordAsync(user.Id, oldPassword, newPassword);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("The old password cannot be equal to the new one."));
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ShouldReturnSuccess_WhenValidInputsAreProvided()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        string oldPassword = "old-P@$$w0rd";
        string newPassword = "new-P@$$w0rd";

        await userService.AddUserPasswordAsync(user.Id, oldPassword);

        // Act
        var result = await userService.ChangeUserPasswordAsync(user.Id, oldPassword, newPassword);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ChangeUserPasswordAsync_ShouldReturnFailedResult_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        string oldPassword = "old-P@$$w0rd";
        string newPassword = "new-P@$$w0rd";

        await userService.AddUserPasswordAsync(user.Id, oldPassword);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.ChangeUserPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await userService.ChangeUserPasswordAsync(user.Id, oldPassword, newPassword);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("Failed to change user password: Database error."));
    }



    [Fact]
    public async Task AddUserPasswordAsync_ShouldReturnFailedResult_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string newPassword = "new-P@$$w0rd";

        // Act
        var result = await userService.AddUserPasswordAsync(null!, newPassword);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User ID cannot be null or empty."));
    }

    [Fact]
    public async Task AddUserPasswordAsync_ShouldReturnFailedResult_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";
        string newPassword = "new-P@$$w0rd";

        // Act
        var result = await userService.AddUserPasswordAsync(nonExistentUserID, newPassword);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User not found."));
    }

    [Fact]
    public async Task AddUserPasswordAsync_ShouldReturnFailedResult_WhenNewPasswordIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.AddUserPasswordAsync(user.Id, null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("Password cannot be null or empty."));
    }

    [Fact]
    public async Task AddUserPasswordAsync_ShouldReturnSuccess_WhenValidInputsAreProvided()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        string newPassword = "new-P@$$w0rd";

        // Act
        var result = await userService.AddUserPasswordAsync(user.Id, newPassword);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AddUserPasswordAsync_ShouldReturnFailedResult_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.AddUserPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        string newPassword = "new-P@$$w0rd";

        // Act
        var result = await userService.AddUserPasswordAsync(user.Id, newPassword);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("Failed to add user password: Database error."));
    }


    [Fact]
    public async Task HasUserPasswordAsync_ShouldThrowException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await userService.HasUserPasswordAsync(null!));
        Assert.Contains("User ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldThrowException_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = Guid.NewGuid().ToString();

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await userService.HasUserPasswordAsync(nonExistentUserID));
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldReturnTrue_WhenUserHasPassword()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        string password = "P@$$w0rd";
        await userService.CreateUserAsync(user, password);

        // Act
        var result = await userService.HasUserPasswordAsync(user.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldReturnFalse_WhenUserDoesNotHavePassword()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.HasUserPasswordAsync(user.Id);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.HasUserPasswordAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await userService.HasUserPasswordAsync(user.Id));
        Assert.Equal("Database error", ex.Message);
    }

    #endregion

    #region Roles

    [Fact]
    public async Task GetUserRolesAsync_ShouldThrowArgumentNullOrEmptyException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUserRolesAsync(null!));
        Assert.Equal("User ID", ex.ParamName);
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldThrowNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => userService.GetUserRolesAsync(nonExistentUserID));
        Assert.Equal("User", ex.ParamName);
    }

    [Fact]
    public async Task GetUserRolesAsync_ShouldReturnUserRoles_WhenValidUserIdIsProvided()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var roles = new[] { Roles.UserRole, Roles.AdminRole };
        await userService.AddRolesToUserAsync(user.Id, roles);

        // Act
        var result = await userService.GetUserRolesAsync(user.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(Roles.UserRole, result);
        Assert.Contains(Roles.AdminRole, result);
        Assert.Equal(roles.Length, result.Count());
    }

    [Fact]
    public async Task AddUserPasswordAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var roles = new[] { Roles.UserRole, Roles.AdminRole };
        await userService.AddRolesToUserAsync(user.Id, roles);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.GetUserRolesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));


        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.GetUserRolesAsync(user.Id));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task GetUsersByRoleAsync_ShouldThrow_WhenRoleNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(() => userService.GetUsersByRoleAsync(null!));
        Assert.Equal("Role name", ex.ParamName);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldThrow_WhenRoleDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentRoleName = "nonExistentRoleName";

        // Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(() => userService.GetUsersByRoleAsync(nonExistentRoleName));
        Assert.Equal("Role", ex.ParamName);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnUsers_WhenValidRoleNameIsProvided()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user_User = CreateUser("User", "user@gmail.com");
        await userService.AddUserAsync(user_User);

        var admin_User = CreateUser("Admin", "admin@gmail.com");
        await userService.AddUserAsync(admin_User);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        await userService.AddRolesToUserAsync(user_User.Id, new[] { Roles.UserRole });
        await userService.AddRolesToUserAsync(admin_User.Id, new[] { Roles.AdminRole, Roles.UserRole });

        // Act
        var resultByUserRole = await userService.GetUsersByRoleAsync(Roles.UserRole);
        var resultByAdminRole = await userService.GetUsersByRoleAsync(Roles.AdminRole);

        // Assert
        Assert.NotNull(resultByUserRole);
        Assert.Contains(user_User, resultByUserRole);
        Assert.Contains(admin_User, resultByUserRole);
        Assert.Equal(2, resultByUserRole.Count());

        Assert.NotNull(resultByAdminRole);
        Assert.Contains(admin_User, resultByAdminRole);
        Assert.Single(resultByAdminRole);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var roles = new[] { Roles.UserRole, Roles.AdminRole };
        await userService.AddRolesToUserAsync(user.Id, roles);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.GetUsersAsync())
            .ReturnsAsync(() => new[] { user }.AsQueryable());

        mockUserRepository
            .Setup(repo => repo.GetUserRolesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => userService.GetUsersByRoleAsync(Roles.UserRole));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task AddRolesToUserAsync_ShouldReturnFailedResult_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string[] roles = { Roles.AdminRole };

        // Act
        var result = await userService.AddRolesToUserAsync(null!, roles);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User ID cannot be null or empty."));
    }

    [Fact]
    public async Task AddRolesToUserAsync_ShouldReturnFailedResult_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";
        string[] roles = { Roles.AdminRole };

        // Act
        var result = await userService.AddRolesToUserAsync(nonExistentUserID, roles);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User not found."));
    }

    [Fact]
    public async Task AddRolesToUserAsync_ShouldReturnFailedResult_WhenRolesArrayIsEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.AddRolesToUserAsync(user.Id, Array.Empty<string>());

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User cannot have no roles."));
    }

    [Fact]
    public async Task AddRolesToUserAsync_ShouldReturnFailedResult_WhenRoleDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        var nonExistentRole = "nonExistentRole";

        // Act
        var result = await userService.AddRolesToUserAsync(user.Id, new[] { nonExistentRole });

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists($"Role '{nonExistentRole}' not found."));
    }

    [Fact]
    public async Task AddRolesToUserAsync_ShouldReturnSuccess_WhenValidInputsAreProvided()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);
        string[] roles = { Roles.AdminRole };

        // Act
        var result = await userService.AddRolesToUserAsync(user.Id, roles);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AddRolesToUserAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        string[] roles = { Roles.AdminRole };

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.AddRolesToUserAsync(It.IsAny<string>(), It.IsAny<string[]>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await userService.AddRolesToUserAsync(user.Id, roles);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("Database error."));
    }



    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldReturnFailedResult_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string roleName = Roles.AdminRole;

        // Act
        var result = await userService.DeleteRoleFromUserAsync(null!, roleName);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User ID cannot be null or empty."));
    }

    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldReturnFailedResult_WhenUserDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        string nonExistentUserID = "non-existent-userId";
        string roleName = Roles.AdminRole;

        // Act
        var result = await userService.DeleteRoleFromUserAsync(nonExistentUserID, roleName);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("User not found."));
    }

    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldReturnFailedResult_WhenRoleNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await userService.DeleteRoleFromUserAsync(user.Id, null!);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("Role name cannot be null or empty."));
    }

    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldReturnFailedResult_WhenRoleDoesNotExist()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);
        string nonExistentRole = "nonExistentRole";

        // Act
        var result = await userService.DeleteRoleFromUserAsync(user.Id, nonExistentRole);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("Role not found."));
    }

    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldReturnFailedResult_WhenUserDoesNotHaveThisRole()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var roles = new[] { Roles.UserRole };
        await userService.AddRolesToUserAsync(user.Id, roles);

        // Act
        var result = await userService.DeleteRoleFromUserAsync(user.Id, Roles.AdminRole);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists($"User does not have '{Roles.AdminRole}' role"));
    }

    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldReturnSuccess_WhenValidInputsAreProvided()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var roles = new[] { Roles.AdminRole, Roles.UserRole };
        await userService.AddRolesToUserAsync(user.Id, roles);

        // Act
        var result = await userService.DeleteRoleFromUserAsync(user.Id, Roles.AdminRole);

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();

        var userManager = IdentityHelper.GetUserManager(db);
        var mockUserRepository = new Mock<UserRepository>(userManager, db);
        var roleRepository = GetRoleRepository(db);
        var userService = new UserService(mockUserRepository.Object, roleRepository);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var roles = new[] { Roles.AdminRole, Roles.UserRole };
        await userService.AddRolesToUserAsync(user.Id, roles);

        mockUserRepository
            .Setup(repo => repo.UserExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        mockUserRepository
            .Setup(repo => repo.GetUserRolesAsync(It.IsAny<string>()))
            .ReturnsAsync(() => roles);

        mockUserRepository
            .Setup(repo => repo.DeleteRoleFromUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await userService.DeleteRoleFromUserAsync(user.Id, Roles.AdminRole);

        // Assert
        Assert.False(result.Succeeded);
        Assert.True(result.ErrorExists("Database error"));
    }

    #endregion
}
