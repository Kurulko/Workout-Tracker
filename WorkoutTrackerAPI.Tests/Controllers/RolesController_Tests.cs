using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Controllers;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Extentions;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Repositories.UserRepositories;
using WorkoutTracker.API.Services.RoleServices;
using WorkoutTracker.API.Tests;
using WorkoutTracker.API.Tests.Controllers;
using Xunit;

namespace RoleTrackerAPI.Tests.Controllers;

public class RolesController_Tests : APIController_Tests
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

    static RolesController GetRolesController(WorkoutDbContext db)
    {
        var roleService = GetRoleService(db);
        return new RolesController(roleService);
    }

    [Fact]
    public async Task GetRolesAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var roles = GetValidRoles();
        foreach (var role in roles)
        {
            await roleService.AddRoleAsync(role);
        }

        // Act
        var result = await rolesController.GetRolesAsync();

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<IdentityRole>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(okResult.Value.TotalCount, roles.Count());
    }

    [Fact]
    public async Task GetRolesAsync_ShouldReturnFilteredResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var roles = GetValidRoles();;
        foreach (var role in roles)
        {
            await roleService.AddRoleAsync(role);
        }

        // Act
        var result = await rolesController.GetRolesAsync(pageSize: 2);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<IdentityRole>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetRolesAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        // Act
        var result = await rolesController.GetRolesAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetRolesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.GetRolesAsync())
            .ThrowsAsync(new Exception("Failed to get roles."));

        // Act
        var result = await rolesController.GetRolesAsync();

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get roles.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetRolesAsync_ShouldReturnBadRequest_WhenRolesNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.GetRolesAsync())
            .ReturnsAsync(default(IQueryable<IdentityRole>)!);

        // Act
        var result = await rolesController.GetRolesAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Roles not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task GetRoleByIdAsync_ShouldReturnBadRequest_WhenRoleIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        // Act
        var result = await rolesController.GetRoleByIdAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Role ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetRoleByIdAsync_ShouldReturnRoleById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await rolesController.GetRoleByIdAsync(role.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<IdentityRole>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(role, okResult.Value);
    }

    [Fact]
    public async Task GetRoleByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.GetRoleByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get role by ID."));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await rolesController.GetRoleByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get role by ID.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetRoleByIdAsync_ShouldReturnBadRequest_WhenRoleNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        var notFoundID = Guid.NewGuid().ToString();

        // Act
        var result = await rolesController.GetRoleByIdAsync(notFoundID);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Role not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task GetRoleByNameAsync_ShouldReturnBadRequest_WhenRoleNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        // Act
        var result = await rolesController.GetRoleByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Role name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetRoleByNameAsync_ShouldReturnRoleByName_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await rolesController.GetRoleByNameAsync(role.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<IdentityRole>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(role, okResult.Value);
    }

    [Fact]
    public async Task GetRoleByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.GetRoleByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get role by name."));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await rolesController.GetRoleByNameAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get role by name.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetRoleByNameAsync_ShouldReturnBadRequest_WhenRoleNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        var notFoundName = "notFoundName";

        // Act
        var result = await rolesController.GetRoleByNameAsync(notFoundName);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Role not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task AddRoleAsync_ShouldCreateRole_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        var role = GetValidRole();

        // Act
        var result = await rolesController.AddRoleAsync(role);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(RolesController.GetRoleByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.Equal(role, createdAtActionResult.Value);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnBadRequest_WhenRoleIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        // Act
        var result = await rolesController.AddRoleAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Role entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddRoleAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.AddRoleAsync(It.IsAny<IdentityRole>()))
            .ThrowsAsync(new Exception("Failed to add role."));

        var role = GetValidRole();

        // Act
        var result = await rolesController.AddRoleAsync(role);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to add role.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task UpdateRoleAsync_ShouldUpdateRole_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        role.Name = "New Name";

        // Act
        var result = await rolesController.UpdateRoleAsync(role.Id, role);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnBadRequest_WhenInvalidRoleIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        role.Name = "New Name";

        // Act
        var result = await rolesController.UpdateRoleAsync(null!, role);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnBadRequest_WhenRoleIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await rolesController.UpdateRoleAsync(role.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnBadRequest_WhenIDsDoNotMatch()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await rolesController.UpdateRoleAsync(role.Id + 1, role);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role IDs do not match.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateRoleAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.UpdateRoleAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResultExtentions.Failed("Failed to update role."));

        var role = GetValidRole();

        // Act
        var result = await rolesController.UpdateRoleAsync(role.Id, role);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Failed to update role.", (badRequestResult.Value as string[])!);
    }



    [Fact]
    public async Task DeleteRoleAsync_ShouldDeleteRole_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await rolesController.DeleteRoleAsync(role.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldReturnBadRequest_WhenRoleIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        // Act
        var result = await rolesController.DeleteRoleAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Role ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteRoleAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.DeleteRoleAsync(It.IsAny<string>()))
            .ReturnsAsync(IdentityResultExtentions.Failed("Failed to delete role."));

        var role = GetValidRole();

        // Act
        var result = await rolesController.DeleteRoleAsync(role.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains("Failed to delete role.", (badRequestResult.Value as string[])!);
    }


    [Fact]
    public async Task RoleExistsAsync_ShouldReturnBadRequest_WhenRoleIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        // Act
        var result = await rolesController.RoleExistsAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Role ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await rolesController.RoleExistsAsync(role.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var notFoundRoleID = Guid.NewGuid().ToString();

        // Act
        var result = await rolesController.RoleExistsAsync(notFoundRoleID);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task RoleExistsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.RoleExistsAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await rolesController.RoleExistsAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task RoleExistsByNameAsync_ShouldReturnBadRequest_WhenRoleNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        // Act
        var result = await rolesController.RoleExistsByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Role name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task RoleExistsByNameAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await rolesController.RoleExistsByNameAsync(role.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task RoleExistsByNameAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var notFoundRoleName = "notFoundRoleName";

        // Act
        var result = await rolesController.RoleExistsByNameAsync(notFoundRoleName);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task RoleExistsByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.RoleExistsByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultRoleName = "defaultRoleName";

        // Act
        var result = await rolesController.RoleExistsByNameAsync(defaultRoleName);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task GetRoleNameByIdAsync_ShouldReturnBadRequest_WhenRoleIDIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        // Act
        var result = await rolesController.GetRoleNameByIdAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Role ID is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetRoleNameByIdAsync_ShouldReturnRoleById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole();
        await roleService.AddRoleAsync(role);

        // Act
        var result = await rolesController.GetRoleNameByIdAsync(role.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<string>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(role.Name, okResult.Value);
    }

    [Fact]
    public async Task GetRoleNameByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.GetRoleNameByIdAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get role name by ID."));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await rolesController.GetRoleNameByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get role name by ID.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetRoleNameByIdAsync_ShouldReturnBadRequest_WhenRoleNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        var notFoundID = Guid.NewGuid().ToString();

        // Act
        var result = await rolesController.GetRoleNameByIdAsync(notFoundID);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Role not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task GetRoleIdByNameAsync_ShouldReturnBadRequest_WhenRoleNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        // Act
        var result = await rolesController.GetRoleIdByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Role name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetRoleIdByNameAsync_ShouldReturnRoleByName_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleService = GetRoleService(db);
        var rolesController = GetRolesController(db);

        var role = GetValidRole(); ;
        await roleService.AddRoleAsync(role);

        // Act
        var result = await rolesController.GetRoleIdByNameAsync(role.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<string>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(role.Id, okResult.Value);
    }

    [Fact]
    public async Task GetRoleIdByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockRoleService = new Mock<IRoleService>();
        var rolesController = new RolesController(mockRoleService.Object);

        mockRoleService
            .Setup(x => x.GetRoleByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Failed to get role ID by name."));

        var defaultID = Guid.NewGuid().ToString();

        // Act
        var result = await rolesController.GetRoleByNameAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Failed to get role ID by name.", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetRoleIdByNameAsync_ShouldReturnBadRequest_WhenRoleNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var rolesController = GetRolesController(db);

        var notFoundName = "notFoundName";

        // Act
        var result = await rolesController.GetRoleIdByNameAsync(notFoundName);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Role not found.", badRequestResult.Value);
    }
}
