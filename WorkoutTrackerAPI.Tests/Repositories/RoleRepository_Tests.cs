using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using Xunit;
using Microsoft.AspNetCore.Identity;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Tests.Repositories;

public class RoleRepository_Tests : BaseTests
{
    IdentityRole GetValidRole()
        => new IdentityRole("User");

    IdentityRole[] GetValidRoles()
        => new[] {
            new IdentityRole("Admin"),
            new IdentityRole("User")
        };

    [Fact]
    public async Task AddRole_ShouldReturnNewRole()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();

        //Act
        var newRole = await roleRepository.AddRoleAsync(validRole);

        //Assert
        Assert.NotNull(newRole);

        var roleById = await roleRepository.GetRoleByIdAsync(newRole.Id);
        Assert.NotNull(roleById);

        Assert.Equal(newRole, roleById);
    }

    [Fact]
    public async Task AddRole_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();
        await roleRepository.AddRoleAsync(validRole);

        var sameValidRole = GetValidRole();

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await roleRepository.AddRoleAsync(sameValidRole));
        Assert.Equal("Role name must be unique.", ex.Message);
    }

    [Fact]
    public async Task DeleteRole_ShouldDeleteRoleSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();
        await roleRepository.AddRoleAsync(validRole);

        //Act
        var result = await roleRepository.DeleteRoleAsync(validRole.Id);

        //Assert
        Assert.True(result.Succeeded);

        var roleById = await roleRepository.GetRoleByIdAsync(validRole.Id);
        Assert.Null(roleById);
    }

    [Fact]
    public async Task DeleteRole_RoleNotFound()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        //Act
        var result = await roleRepository.DeleteRoleAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.False(result.Succeeded);

        var isRoleNotFound = result.ErrorExists("Role not found.");
        Assert.True(isRoleNotFound);
    }

    [Fact]
    public async Task DeleteRole_IncorrectRoleID()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        //Act
        var result = await roleRepository.DeleteRoleAsync(string.Empty);

        //Assert
        Assert.False(result.Succeeded);

        var isRoleIDIsNullOrEmpty = result.ErrorExists("Role ID cannot not be null or empty.");
        Assert.True(isRoleIDIsNullOrEmpty);
    }

    [Fact]
    public async Task GetRoles_ShouldReturnRoles()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRoles = GetValidRoles();
        foreach (var role in validRoles)
            await roleRepository.AddRoleAsync(role);

        //Act
        var roles = await roleRepository.GetRolesAsync();

        //Assert
        Assert.NotNull(roles);
        Assert.NotEmpty(roles);
        Assert.Equal(validRoles.Length, roles.Count());
    }

    [Fact]
    public async Task GetRoles_ShouldReturnEmpty()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        //Act
        var roles = await roleRepository.GetRolesAsync();

        //Assert
        Assert.NotNull(roles);
        Assert.Empty(roles);
    }

    [Fact]
    public async Task GetRoleById_ShouldReturnRoleById()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();
        await roleRepository.AddRoleAsync(validRole);

        //Act
        var roleById = await roleRepository.GetRoleByIdAsync(validRole.Id);

        //Assert
        Assert.NotNull(roleById);
        Assert.Equal(validRole, roleById);
    }

    [Fact]
    public async Task GetRoleById_ShouldReturnNull()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        //Act
        var roleById = await roleRepository.GetRoleByIdAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.Null(roleById);
    }

    [Fact]
    public async Task GetRoleByName_ShouldReturnRoleByName()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();
        await roleRepository.AddRoleAsync(validRole);

        //Act
        var roleByName = await roleRepository.GetRoleByNameAsync(validRole.Name);

        //Assert
        Assert.NotNull(roleByName);
        Assert.Equal(validRole, roleByName);
    }

    [Fact]
    public async Task GetRoleByName_ShouldReturnNull()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        //Act
        var roleByName = await roleRepository.GetRoleByNameAsync("Non-existence name");

        //Assert
        Assert.Null(roleByName);
    }

    [Fact]
    public async Task RoleExists_ShouldReturnTrue()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();
        await roleRepository.AddRoleAsync(validRole);

        //Act
        var roleExists = await roleRepository.RoleExistsAsync(validRole.Id);

        //Assert
        Assert.True(roleExists);
    }

    [Fact]
    public async Task RoleExists_ShouldReturnFalse()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        //Act
        var roleExists = await roleRepository.RoleExistsAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.False(roleExists);
    }

    [Fact]
    public async Task RoleExistsByName_ShouldReturnTrue()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();
        await roleRepository.AddRoleAsync(validRole);

        //Act
        var roleExistsByName = await roleRepository.RoleExistsByNameAsync(validRole.Name);

        //Assert
        Assert.True(roleExistsByName);
    }

    [Fact]
    public async Task RoleExistsByName_ShouldReturnFalse()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        //Act
        var roleExistsByName = await roleRepository.RoleExistsByNameAsync("Non-existence name");

        //Assert
        Assert.False(roleExistsByName);
    }

    [Fact]
    public async Task UpdateRole_ShouldUpdateRoleSuccessfully()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();
        await roleRepository.AddRoleAsync(validRole);

        validRole.Name = "New Role Name";

        //Act
        await roleRepository.UpdateRoleAsync(validRole);

        //Assert
        var updatedRole = await roleRepository.GetRoleByIdAsync(validRole.Id);

        Assert.NotNull(updatedRole);
        Assert.Equal(updatedRole, validRole);
    }

    [Fact]
    public async Task UpdateRole_RoleNotFound()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();
        await roleRepository.AddRoleAsync(validRole);

        validRole.Id = Guid.NewGuid().ToString();

        //Act
        var result = await roleRepository.UpdateRoleAsync(validRole);

        //Assert
        Assert.False(result.Succeeded);

        var isRoleNotFound = result.ErrorExists("Role not found.");
        Assert.True(isRoleNotFound);
    }

    [Fact]
    public async Task UpdateRole_IncorrectRoleID()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var validRole = GetValidRole();
        await roleRepository.AddRoleAsync(validRole);

        validRole.Id = string.Empty;

        //Act
        var result = await roleRepository.UpdateRoleAsync(validRole);

        //Assert
        Assert.False(result.Succeeded);

        var isRoleIDIsNullOrEmpty = result.ErrorExists("Role ID cannot not be null or empty.");
        Assert.True(isRoleIDIsNullOrEmpty);
    }
}
