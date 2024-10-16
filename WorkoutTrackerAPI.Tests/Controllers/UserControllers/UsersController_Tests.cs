using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.UserServices;
using WorkoutTrackerAPI.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Moq;
using AutoMapper;
using WorkoutTrackerAPI.Profiles;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Extentions;
using Xunit;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Controllers.AuthControllers;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;

namespace WorkoutTrackerAPI.Tests.Controllers.UserControllers;

public class UsersController_Tests : APIController_Tests
{
    readonly Mock<HttpContext> mockHttpContext;
    readonly IMapper mapper;

    public UsersController_Tests()
    {
        mockHttpContext = IdentityHelper.GetMockHttpContext();
        mapper = GetMapper();
    }

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

    static IMapper GetMapper()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<UserProfile>());
        return config.CreateMapper();
    }

    void SetupMockHttpContext(User user)
    {
        mockHttpContext.Setup(x => x.User)
            .Returns(new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            })));
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

    UsersController GetUsersController(WorkoutDbContext db)
    {
        var userService = GetUserService(db);
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        return new UsersController(userService, mapper, mockHttpContextAccessor.Object);
    }

    #region CRUD

    [Fact]
    public async Task GetUsersAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var users = GetValidUsers();
        foreach (var user in users)
        {
            await userService.AddUserAsync(user);
        }

        // Act
        var result = await usersController.GetUsersAsync();

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<UserDTO>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(okResult.Value.TotalCount, users.Count());
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnFilteredResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var users = GetValidUsers();
        foreach (var user in users)
        {
            await userService.AddUserAsync(user);
        }

        // Act
        var result = await usersController.GetUsersAsync(pageSize: 2);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<UserDTO>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetUsersAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUsersAsync())
            .ThrowsAsync(new Exception("Failed to get users."));

        // Act
        var result = await usersController.GetUsersAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get users.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetUsersAsync_ShouldReturnBadRequest_WhenUsersNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUsersAsync())
            .ReturnsAsync(default(IQueryable<User>)!);

        // Act
        var result = await usersController.GetUsersAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Users not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnBadRequest_WhenUserIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetUserByIdAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnUserById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.GetUserByIdAsync(user.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<UserDTO>>(result);
        Assert.NotNull(okResult.Value);

        var userDTO = okResult.Value;
        Assert.True(userDTO.Equals(user));
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user by ID."));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await usersController.GetUserByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get user by ID.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetUserByIdAsync_ShouldReturnBadRequest_WhenUserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var notFoundID = Guid.NewGuid().ToString();

        // Act
        var result = await usersController.GetUserByIdAsync(notFoundID);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnBadRequest_WhenUserNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetUserByUsernameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnUserByUsername_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.GetUserByUsernameAsync(user.UserName);

        // Assert
        var okResult = Assert.IsType<ActionResult<User>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(user, okResult.Value);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserByUsernameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user by name."));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await usersController.GetUserByUsernameAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get user by name.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetUserByUsernameAsync_ShouldReturnBadRequest_WhenUserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var notFoundName = "notFoundName";

        // Act
        var result = await usersController.GetUserByUsernameAsync(notFoundName);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task AddUserAsync_ShouldAddUser_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        var userDTO = mapper.Map<UserCreationDTO>(user);

        // Act
        var result = await usersController.AddUserAsync(userDTO);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UsersController.GetUserByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.IsAssignableFrom<UserDTO>(createdAtActionResult.Value);
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnBadRequest_WhenUserIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.AddUserAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.AddUserAsync(It.IsAny<User>()))
            .ThrowsAsync(new Exception("Failed to add user."));

        var user = GetValidUser();
        var userDTO = mapper.Map<UserCreationDTO>(user);

        // Act
        var result = await usersController.AddUserAsync(userDTO);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal("Internal server error: Failed to add user.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task CreateUserAsync_ShouldCreateUser_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        var userDTO = mapper.Map<UserCreationDTO>(user);
        string password = "P@$$w0rd";

        // Act
        var result = await usersController.CreateUserAsync(userDTO, password);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(UsersController.GetUserByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.IsAssignableFrom<UserDTO>(createdAtActionResult.Value);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnBadRequest_WhenUserIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        string password = "P@$$w0rd";

        // Act
        var result = await usersController.CreateUserAsync(null!, password);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnBadRequest_WhenPasswordIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        var userDTO = mapper.Map<UserCreationDTO>(user);

        // Act
        var result = await usersController.CreateUserAsync(userDTO, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Password is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.CreateUserAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResultExtentions.Failed("Failed to create user"));

        var user = GetValidUser();
        var userDTO = mapper.Map<UserCreationDTO>(user);
        string password = "P@$$w0rd";

        // Act
        var result = await usersController.CreateUserAsync(userDTO, password);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Failed to create user", (badRequestResult.Value as string[])!);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldUpdateUser_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        user.UserName = "New Name";

        // Act
        var result = await usersController.UpdateUserAsync(user.Id, user);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnBadRequest_WhenInvalidUserIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        user.UserName = "New Name";

        // Act
        var result = await usersController.UpdateUserAsync(null!, user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnBadRequest_WhenUserIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.UpdateUserAsync(user.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnBadRequest_WhenIDsDoNotMatch()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.UpdateUserAsync(user.Id + 1, user);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User IDs do not match.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(IdentityResultExtentions.Failed("Failed to update user."));

        var user = GetValidUser();

        // Act
        var result = await usersController.UpdateUserAsync(user.Id, user);

        // Assert
        Assert.IsAssignableFrom<ForbidResult>(result);
    }



    [Fact]
    public async Task DeleteUserAsync_ShouldDeleteUser_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.DeleteUserAsync(user.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnBadRequest_WhenUserIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.DeleteUserAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.DeleteUserAsync(It.IsAny<string>()))
            .ReturnsAsync(IdentityResultExtentions.Failed("Failed to delete user."));

        var user = GetValidUser();

        // Act
        var result = await usersController.DeleteUserAsync(user.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Failed to delete user.", (badRequestResult.Value as string[])!);
    }


    [Fact]
    public async Task UserExistsAsync_ShouldReturnBadRequest_WhenUserIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.UserExistsAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task UserExistsAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.UserExistsAsync(user.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task UserExistsAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var notFoundUserID = Guid.NewGuid().ToString();

        // Act
        var result = await usersController.UserExistsAsync(notFoundUserID);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task UserExistsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.UserExistsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await usersController.UserExistsAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task UserExistsByUsernameAsync_ShouldReturnBadRequest_WhenUserNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.UserExistsByUsernameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task UserExistsByUsernameAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.UserExistsByUsernameAsync(user.UserName);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task UserExistsByUsernameAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var notFoundUserName = "notFoundUserName";

        // Act
        var result = await usersController.UserExistsByUsernameAsync(notFoundUserName);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task UserExistsByUsernameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.UserExistsByUsernameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultUserName = "defaultUserName";

        // Act
        var result = await usersController.UserExistsByUsernameAsync(defaultUserName);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task GetUserNameByIdAsync_ShouldReturnBadRequest_WhenUserIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetUserNameByIdAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUserNameByIdAsync_ShouldReturnUserById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.GetUserNameByIdAsync(user.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<string>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(user.UserName, okResult.Value);
    }

    [Fact]
    public async Task GetUserNameByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserNameByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user name by ID."));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await usersController.GetUserNameByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get user name by ID.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetUserNameByIdAsync_ShouldReturnBadRequest_WhenUserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var notFoundID = Guid.NewGuid().ToString();

        // Act
        var result = await usersController.GetUserNameByIdAsync(notFoundID);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task GetUserIdByUsernameAsync_ShouldReturnBadRequest_WhenUserNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetUserIdByUsernameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUserIdByUsernameAsync_ShouldReturnUserByUsername_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser(); ;
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.GetUserIdByUsernameAsync(user.UserName);

        // Assert
        var okResult = Assert.IsType<ActionResult<string>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(user.Id, okResult.Value);
    }

    [Fact]
    public async Task GetUserIdByUsernameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserByUsernameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user ID by name."));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await usersController.GetUserByUsernameAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get user ID by name.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetUserIdByUsernameAsync_ShouldReturnBadRequest_WhenUserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var notFoundName = "notFoundName";

        // Act
        var result = await usersController.GetUserIdByUsernameAsync(notFoundName);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User not found.", badRequestResult.Value);
    }

    #endregion

    #region User Models

    [Fact]
    public async Task GetCurrentUserMuscleSizesAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        SetupMockHttpContext(user);

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
        var result = await usersController.GetCurrentUserMuscleSizesAsync();

        // Assert
        var userMuscleSize = result.Value;
        Assert.NotNull(userMuscleSize);
        Assert.Equal(muscleSizes.Count(), userMuscleSize.TotalCount);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizesAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetCurrentUserMuscleSizesAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizesAsync_ShouldReturnBadRequest_WhenUserMuscleSizeaNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserMuscleSizesAsync(It.IsAny<string>()))
            .ReturnsAsync(default(IQueryable<MuscleSize>)!);

        // Act
        var result = await usersController.GetCurrentUserMuscleSizesAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User muscle sizes not found.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserMuscleSizesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user muscle sizes."));

        // Act
        var result = await usersController.GetCurrentUserMuscleSizesAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get user muscle sizes.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }



    [Fact]
    public async Task GetCurrentUserBodyWeightsAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        SetupMockHttpContext(user);

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
        var result = await usersController.GetCurrentUserBodyWeightsAsync();

        // Assert
        var userBodyWeights = result.Value;
        Assert.NotNull(userBodyWeights);
        Assert.Equal(bodyWeights.Count(), userBodyWeights.TotalCount);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightsAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetCurrentUserBodyWeightsAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightsAsync_ShouldReturnBadRequest_WhenUserMuscleSizeaNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserBodyWeightsAsync(It.IsAny<string>()))
            .ReturnsAsync(default(IQueryable<BodyWeight>)!);

        // Act
        var result = await usersController.GetCurrentUserBodyWeightsAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User body weights not found.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserBodyWeightsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user body weights."));

        // Act
        var result = await usersController.GetCurrentUserBodyWeightsAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get user body weights.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        SetupMockHttpContext(user);

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
        var result = await usersController.GetCurrentUserWorkoutsAsync();

        // Assert
        var userWorkouts = result.Value;
        Assert.NotNull(userWorkouts);
        Assert.Equal(workouts.Count(), userWorkouts.TotalCount);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetCurrentUserWorkoutsAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ShouldReturnBadRequest_WhenUserMuscleSizeaNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserWorkoutsAsync(It.IsAny<string>()))
            .ReturnsAsync(default(IQueryable<Workout>)!);

        // Act
        var result = await usersController.GetCurrentUserWorkoutsAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User workouts not found.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserWorkoutsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user workouts."));

        // Act
        var result = await usersController.GetCurrentUserWorkoutsAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get user workouts.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task GetCurrentUserCreatedExercisesAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        SetupMockHttpContext(user);

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
        var result = await usersController.GetCurrentUserCreatedExercisesAsync();

        // Assert
        var userExercises = result.Value;
        Assert.NotNull(userExercises);
        Assert.Equal(exercises.Count(), userExercises.TotalCount);
    }

    [Fact]
    public async Task GetCurrentUserCreatedExercisesAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetCurrentUserCreatedExercisesAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserCreatedExercisesAsync_ShouldReturnBadRequest_WhenUserMuscleSizeaNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserCreatedExercisesAsync(It.IsAny<string>()))
            .ReturnsAsync(default(IQueryable<Exercise>)!);

        // Act
        var result = await usersController.GetCurrentUserCreatedExercisesAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User created exercises not found.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserCreatedExercisesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserCreatedExercisesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user created exercises."));

        // Act
        var result = await usersController.GetCurrentUserCreatedExercisesAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get user created exercises.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        SetupMockHttpContext(user);

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
        var result = await usersController.GetCurrentUserExerciseRecordsAsync();

        // Assert
        var userExerciseRecords = result.Value;
        Assert.NotNull(userExerciseRecords);
        Assert.Equal(exerciseRecords.Count(), userExerciseRecords.TotalCount);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetCurrentUserExerciseRecordsAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnBadRequest_WhenUserMuscleSizeaNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserExerciseRecordsAsync(It.IsAny<string>()))
            .ReturnsAsync(default(IQueryable<ExerciseRecord>)!);

        // Act
        var result = await usersController.GetCurrentUserExerciseRecordsAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User exercise records not found.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserExerciseRecordsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user exercise records."));

        // Act
        var result = await usersController.GetCurrentUserExerciseRecordsAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get user exercise records.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    #endregion

    #region Password

    [Fact]
    public async Task ChangeCurrentUserPasswordAsync_ShouldReturnOkResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        SetupMockHttpContext(user);

        string oldPassword = "old-P@$$w0rd";
        await userService.AddUserPasswordAsync(user.Id, oldPassword);

        string newPassword = "new-P@$$w0rd";
        var passwordModel = new PasswordModel()
        {
            OldPassword = oldPassword,
            NewPassword = newPassword,
            ConfirmNewPassword = newPassword
        };

        // Act
        var result = await usersController.ChangeCurrentUserPasswordAsync(passwordModel);

        // Assert
        Assert.IsAssignableFrom<OkResult>(result);
    }

    [Fact]
    public async Task ChangeCurrentUserPasswordAsync_ShouldReturnBadRequest_WhenPasswordModelIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.ChangeCurrentUserPasswordAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Password entry is null.", badRequestResult.Value);
    }
    
    [Fact]
    public async Task ChangeCurrentUserPasswordAsync_ShouldReturnBadRequest_WhenPasswordModelIsInvalid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        usersController.ModelState.AddModelError("Password", "Required");
        usersController.ModelState.AddModelError("Password", "Minimum length 8 required");

        var defaultPasswordModel = new PasswordModel();

        // Act
        var result = await usersController.ChangeCurrentUserPasswordAsync(defaultPasswordModel);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        Assert.IsAssignableFrom<string[]>(badRequestResult.Value);

        var errors = (badRequestResult.Value as string[])!;
        Assert.Equal(2, errors.Length);
        Assert.Contains("Required", errors);
        Assert.Contains("Minimum length 8 required", errors);
    }


    [Fact]
    public async Task ChangeCurrentUserPasswordAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.ChangeUserPasswordAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResultExtentions.Failed("Failed to change user password."));

        var defaultPasswordModel = new PasswordModel();

        // Act
        var result = await usersController.ChangeCurrentUserPasswordAsync(defaultPasswordModel);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Failed to change user password.", (badRequestResult.Value as string[])!);
    }



    [Fact]
    public async Task CreateUserPasswordAsync_ShouldReturnOkResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        string password = "P@$$w0rd";
        
        // Act
        var result = await usersController.CreateUserPasswordAsync(user.Id, password);

        // Assert
        Assert.IsAssignableFrom<OkResult>(result);
    }

    [Fact]
    public async Task CreateUserPasswordAsync_ShouldReturnBadRequest_WhenUserIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        string password = "P@$$w0rd";

        // Act
        var result = await usersController.CreateUserPasswordAsync(null!, password);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateUserPasswordAsync_ShouldReturnBadRequest_WhenPasswordIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        // Act
        var result = await usersController.CreateUserPasswordAsync(user.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Password is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task CreateUserPasswordAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.AddUserPasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResultExtentions.Failed("Failed to create user password."));

        var user = GetValidUser();
        string password = "P@$$w0rd";

        // Act
        var result = await usersController.CreateUserPasswordAsync(user.Id, password);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Failed to create user password.", (badRequestResult.Value as string[])!);
    }


    [Fact]
    public async Task HasUserPasswordAsync_ShouldReturnOkTrueResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        string password = "P@$$w0rd";
        await userService.CreateUserAsync(user, password);

        // Act
        var result = await usersController.HasUserPasswordAsync(user.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult<bool>>(result);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldReturnOkFalseResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);
        
        // Act
        var result = await usersController.HasUserPasswordAsync(user.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult<bool>>(result);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldReturnBadRequest_WhenUserIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.HasUserPasswordAsync(null!);

        // Assert
        var objectResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("User ID is null or empty.", objectResult.Value);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.HasUserPasswordAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var user = GetValidUser();

        // Act
        var result = await usersController.HasUserPasswordAsync(user.Id);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Contains("Database error", (objectResult.Value as string)!);
        Assert.Equal(500, objectResult.StatusCode);
    }

    #endregion

    #region Roles

    [Fact]
    public async Task GetCurrentUserRolesAsync_ShouldCurrentUserRoles_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        SetupMockHttpContext(user);
        await WorkoutContextFactory.InitializeRolesAsync(db);

        var userRoles = new[] { Roles.UserRole, Roles.AdminRole };
        await userService.AddRolesToUserAsync(user.Id, userRoles);

        // Act
        var result = await usersController.GetCurrentUserRolesAsync();

        // Assert
        Assert.Equal(userRoles, result.Value!);
    }

    [Fact]
    public async Task GetCurrentUserRolesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUserRolesAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get user roles."));

        // Act
        var result = await usersController.GetCurrentUserRolesAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Contains("Failed to get user roles.", (badRequestResult.Value as string)!);
        Assert.Equal(500, badRequestResult.StatusCode);
    }



    [Fact]
    public async Task AddRolesToUserAsync_ShouldOkResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        SetupMockHttpContext(user);
        await WorkoutContextFactory.InitializeRolesAsync(db);

        var userRoles = new[] { Roles.UserRole, Roles.AdminRole };
       
        // Act
        var result = await usersController.AddRolesToUserAsync(user.Id, userRoles);

        // Assert
        Assert.IsAssignableFrom<OkResult>(result);
    }

    [Fact]
    public async Task AddRolesToUserAsync_ShouldReturnBadRequest_WhenUserIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var userRoles = new[] { Roles.UserRole, Roles.AdminRole };

        // Act
        var result = await usersController.AddRolesToUserAsync(null!, userRoles);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddRolesToUserAsync_ShouldReturnBadRequest_WhenUserHaveNoRoles()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var user = GetValidUser();

        // Act
        var result = await usersController.AddRolesToUserAsync(user.Id, Array.Empty<string>());

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User cannot have no roles.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddRolesToUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.AddRolesToUserAsync(It.IsAny<string>(), It.IsAny<string[]>()))
            .ReturnsAsync(IdentityResultExtentions.Failed("Failed add get roles to user."));

        var user = GetValidUser();
        var userRoles = new[] { Roles.UserRole, Roles.AdminRole };

        // Act
        var result = await usersController.AddRolesToUserAsync(user.Id, userRoles);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Failed add get roles to user.", (badRequestResult.Value as string[])!);
    }


    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldOkResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        var user = GetValidUser();
        await userService.AddUserAsync(user);

        SetupMockHttpContext(user);
        await WorkoutContextFactory.InitializeRolesAsync(db);

        var userRoles = new[] { Roles.UserRole, Roles.AdminRole };
        await usersController.AddRolesToUserAsync(user.Id, userRoles);

        // Act
        var result = await usersController.DeleteRoleFromUserAsync(user.Id, Roles.AdminRole);

        // Assert
        Assert.IsAssignableFrom<OkResult>(result);
    }

    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldReturnBadRequest_WhenUserIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.DeleteRoleFromUserAsync(null!, Roles.AdminRole);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldReturnBadRequest_WhenUserHaveNoRoles()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        var user = GetValidUser();

        // Act
        var result = await usersController.DeleteRoleFromUserAsync(user.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteRoleFromUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.DeleteRoleFromUserAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResultExtentions.Failed("Failed add delete role from user."));

        var user = GetValidUser();

        // Act
        var result = await usersController.DeleteRoleFromUserAsync(user.Id, Roles.AdminRole);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Failed add delete role from user.", (badRequestResult.Value as string[])!);
    }



    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userService = GetUserService(db);
        var usersController = GetUsersController(db);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var users = GetValidUsers();
        foreach (var user in users)
        {
            await userService.AddUserAsync(user);
            await userService.AddRolesToUserAsync(user.Id, new[] { Roles.UserRole });
        }

        // Act
        var result = await usersController.GetUsersByRoleAsync(Roles.UserRole);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<UserDTO>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(users.Count(), okResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetUsersByRoleAsync(Roles.UserRole, pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }
    
    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnBadRequest_WhenRoleNameIsNullOrEmptyx()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var usersController = GetUsersController(db);

        // Act
        var result = await usersController.GetUsersByRoleAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Role name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnBadRequest_WhenUsersNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUsersByRoleAsync(It.IsAny<string>()))
            .ReturnsAsync(default(IEnumerable<User>)!);

        // Act
        var result = await usersController.GetUsersByRoleAsync(Roles.UserRole);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Users not found.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetUsersByRoleAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockUserService = new Mock<IUserService>();
        var mockHttpContextAccessor = IdentityHelper.GetMockHttpContextAccessor(mockHttpContext.Object);
        var mapper = GetMapper();
        var usersController = new UsersController(mockUserService.Object, mapper, mockHttpContextAccessor.Object);

        mockUserService
            .Setup(x => x.GetUsersByRoleAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get users by role."));

        // Act
        var result = await usersController.GetUsersByRoleAsync(Roles.UserRole);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get users by role.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    #endregion
}
