using Microsoft.AspNetCore.Identity;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.RoleServices;
using Moq;
using Xunit;
using WorkoutTrackerAPI.Exceptions;
using System.Data;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Tests.Services;

public class RoleService_Tests : BaseService_Tests
{
    static IdentityRole GetValidRole()
        => new("User");

    static IdentityRole[] GetValidRoles()
        => new[] {
            new IdentityRole("Admin"),
            new IdentityRole("User")
        };

    static IRoleService GetRoleService(WorkoutDbContext db)
    {
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);
        return new RoleService(roleRepository);
    }

    [Fact]
    public async Task AddRole_ShouldReturnNewRole_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var validRole = GetValidRole();

        // Act
        var newRole = await roleService.AddRoleAsync(validRole);

        // Assert
        Assert.NotNull(newRole);
        Assert.Equal(validRole, newRole);
    }

    [Fact]
    public async Task AddRole_ShouldThrowException_WhenRoleIsNull()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<EntryNullException>(async () => await roleService.AddRoleAsync(null!));
        Assert.Equal("Role entry cannot be null.", ex.Message);
    }

    [Fact]
    public async Task AddRole_ShouldThrowException_WhenRoleAlreadyExists()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var validRole = GetValidRole();
        await roleService.AddRoleAsync(validRole);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await roleService.AddRoleAsync(validRole));
        Assert.Equal("Role already exists.", ex.Message);
    }

    [Fact]
    public async Task AddRole_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        var validRole = GetValidRole();

        roleRepositoryMock
            .Setup(repo => repo.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => false);

        roleRepositoryMock
            .Setup(repo => repo.AddRoleAsync(It.IsAny<IdentityRole>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await roleService.AddRoleAsync(validRole));
        Assert.Equal("Database error", ex.Message);
    }



    [Fact]
    public async Task DeleteRole_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await roleService.DeleteRoleAsync(role.Id);

        // Assert
        Assert.True(result.Succeeded);

        var roleById = await roleService.GetRoleByIdAsync(role.Id);
        Assert.Null(roleById);
    }

    [Fact]
    public async Task DeleteRole_ShouldReturnFail_WhenInvalidRoleID()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        string invalidRoleID = Guid.NewGuid().ToString();

        // Act
        var result = await roleService.DeleteRoleAsync(invalidRoleID);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("Role not found.");
        Assert.True(errorExists);
    }

    [Fact]
    public async Task DeleteRole_ShouldReturnFail_WhenRoleNotFound()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        string nonExistenceRoleId = Guid.NewGuid().ToString();

        // Act
        var result = await roleService.DeleteRoleAsync(nonExistenceRoleId);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("Role not found.");
        Assert.True(errorExists);
    }

    [Fact]
    public async Task DeleteRole_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        var role = GetValidRole();

        roleRepositoryMock
            .Setup(repo => repo.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        roleRepositoryMock
            .Setup(repo => repo.DeleteRoleAsync(role.Id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await roleService.DeleteRoleAsync(role.Id);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists($"Failed to delete role: Database error.");
        Assert.True(errorExists);
    }


    [Fact]
    public async Task GetRoles_ShouldReturnRoles_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var roles = GetValidRoles();

        foreach (var role in roles)
        {
            await roleService.AddRoleAsync(role);
        }

        // Act
        var addedRoles = await roleService.GetRolesAsync();

        // Assert
        Assert.NotNull(addedRoles);
        Assert.Equal(roles.Count(), addedRoles.Count());
    }

    [Fact]
    public async Task GetRoles_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        roleRepositoryMock
            .Setup(repo => repo.GetRolesAsync())
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await roleService.GetRolesAsync());
        Assert.Equal("Database error", ex.Message);
    }



    [Fact]
    public async Task GetRoleByName_ShouldReturnRoleByName_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var roles = GetValidRoles();
        foreach (var role in roles)
        {
            await roleService.AddRoleAsync(role);
        }

        string roleName = "User";

        // Act
        var returnedRole = await roleService.GetRoleByNameAsync(roleName);

        // Assert
        Assert.NotNull(returnedRole);
        Assert.Equal(roleName, returnedRole.Name);
    }

    [Fact]
    public async Task GetRoleByName_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var roles = GetValidRoles();
        foreach (var role in roles)
        {
            await roleService.AddRoleAsync(role);
        }

        string nonExistenceRole = "Non-existence role";

        // Act
        var returnedRole = await roleService.GetRoleByNameAsync(nonExistenceRole);

        // Assert
        Assert.Null(returnedRole);
    }

    [Fact]
    public async Task GetRoleByName_ShouldThrowException_WhenInvalidName()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var roles = GetValidRoles();
        foreach (var role in roles)
        {
            await roleService.AddRoleAsync(role);
        }

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await roleService.GetRoleByNameAsync(null!));
        Assert.Equal("Role name cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task GetRoleByName_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        var role = GetValidRole();

        roleRepositoryMock
            .Setup(repo => repo.GetRoleByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await roleService.GetRoleByNameAsync(role.Name));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task GetRoleById_ShouldReturnRoleById_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var returnedRole = await roleService.GetRoleByIdAsync(role.Id);

        // Assert
        Assert.NotNull(returnedRole);
        Assert.Equal(role.Id, returnedRole.Id);
    }

    [Fact]
    public async Task GetRoleById_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        string nonExistenceRoleId = Guid.NewGuid().ToString();

        // Act
        var returnedRole = await roleService.GetRoleByIdAsync(nonExistenceRoleId);

        // Assert
        Assert.Null(returnedRole);
    }

    [Fact]
    public async Task GetRoleById_ShouldThrowException_WhenInvalidRoleID()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await roleService.GetRoleByIdAsync(null!));
        Assert.Equal("Role ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task GetRoleById_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        var role = GetValidRole();

        roleRepositoryMock
            .Setup(repo => repo.GetRoleByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await roleService.GetRoleByIdAsync(role.Name));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task UpdateRole_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        role.Name = "New Name";

        // Act
        var result = await roleService.UpdateRoleAsync(role);

        // Assert
        Assert.True(result.Succeeded);

        var roleById = await roleService.GetRoleByIdAsync(role.Id);
        Assert.NotNull(roleById);
        Assert.Equal(role.Name, roleById.Name);
    }

    [Fact]
    public async Task UpdateRole_ShouldReturnFail_WhenRoleIsNull()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        // Act
        var result = await roleService.UpdateRoleAsync(null!);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("Role entry cannot be null.");
        Assert.True(errorExists);
    }

    [Fact]
    public async Task UpdateRole_ShouldReturnFail_WhenInvalidRoleID()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        role.Id = null!;
        role.Name = "New Name";

        // Act
        var result = await roleService.UpdateRoleAsync(role);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("Role ID cannot be null or empty.");
        Assert.True(errorExists);
    }

    [Fact]
    public async Task UpdateRole_ShouldReturnFail_WhenRoleNotFound()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var role = GetValidRole();
        role.Name = "New Name";

        // Act
        var result = await roleService.UpdateRoleAsync(role);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists("Role not found.");
        Assert.True(errorExists);
    }

    [Fact]
    public async Task UpdateRole_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        var role = GetValidRole();

        roleRepositoryMock
            .Setup(repo => repo.RoleExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(() => true);

        roleRepositoryMock
            .Setup(repo => repo.UpdateRoleAsync(It.IsAny<IdentityRole>()))
            .ThrowsAsync(new Exception("Database error"));

        role.Name = "New Name";

        // Act
        var result = await roleService.UpdateRoleAsync(role);

        // Assert
        Assert.False(result.Succeeded);

        bool errorExists = result.ErrorExists($"Failed to update role: Database error.");
        Assert.True(errorExists);
    }



    [Fact]
    public async Task RoleExists_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await roleService.RoleExistsAsync(role.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RoleExists_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        string nonExistenceRoleId = Guid.NewGuid().ToString();

        // Act
        var result = await roleService.RoleExistsAsync(nonExistenceRoleId);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RoleExists_ShouldThrowException_WhenInvalidRoleID()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await roleService.RoleExistsAsync(null!));
        Assert.Equal("Role ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task RoleExists_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        var role = GetValidRole();

        roleRepositoryMock
            .Setup(repo => repo.RoleExistsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await roleService.RoleExistsAsync(role.Id));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task RoleExistsByName_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await roleService.RoleExistsByNameAsync(role.Name);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task RoleExistsByName_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        string nonExistenceName = "Non-existence name";

        // Act
        var result = await roleService.RoleExistsByNameAsync(nonExistenceName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task RoleExistsByName_ShouldThrowException_WhenInvalidRoleID()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await roleService.RoleExistsByNameAsync(null!));
        Assert.Equal("Role name cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task RoleExistsByName_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        var role = GetValidRole();

        roleRepositoryMock
            .Setup(repo => repo.RoleExistsByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await roleService.RoleExistsByNameAsync(role.Name));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task GetRoleIdByName_ShouldReturnRoleIdByName_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var roleId = await roleService.GetRoleIdByNameAsync(role.Name);

        // Assert
        Assert.NotNull(roleId);
        Assert.NotEmpty(roleId);
        Assert.Equal(role.Id, roleId);
    }

    [Fact]
    public async Task GetRoleIdByName_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        string nonExistenceRole = "Non-existence role";

        // Act
        var roleId = await roleService.GetRoleIdByNameAsync(nonExistenceRole);

        // Assert
        Assert.Null(roleId);
    }

    [Fact]
    public async Task GetRoleIdByName_ShouldThrowException_WhenInvalidName()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await roleService.GetRoleIdByNameAsync(null!));
        Assert.Equal("Role name cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task GetRoleIdByName_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        var role = GetValidRole();

        roleRepositoryMock
            .Setup(repo => repo.GetRoleByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await roleService.GetRoleIdByNameAsync(role.Name));
        Assert.Equal("Database error", ex.Message);
    }



    [Fact]
    public async Task GetRoleNameById_ShouldReturnRoleNameById_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var roleName = await roleService.GetRoleNameByIdAsync(role.Id);

        // Assert
        Assert.NotNull(roleName);
        Assert.NotEmpty(roleName);
        Assert.Equal(role.Name, roleName);
    }

    [Fact]
    public async Task GetRoleNameById_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        string nonExistenceRoleId = Guid.NewGuid().ToString();

        // Act
        var roleName = await roleService.GetRoleNameByIdAsync(nonExistenceRoleId);

        // Assert
        Assert.Null(roleName);
    }

    [Fact]
    public async Task GetRoleNameById_ShouldThrowException_WhenInvalidId()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await roleService.GetRoleNameByIdAsync(null!));
        Assert.Equal("Role ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task GetRoleNameById_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();

        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepositoryMock = new Mock<RoleRepository>(roleManager);
        var roleService = new RoleService(roleRepositoryMock.Object);

        var role = GetValidRole();

        roleRepositoryMock
            .Setup(repo => repo.GetRoleByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await roleService.GetRoleNameByIdAsync(role.Name));
        Assert.Equal("Database error", ex.Message);
    }
}
