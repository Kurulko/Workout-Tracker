using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data.Models.UserModels;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Services;
using WorkoutTracker.API.Services.RoleServices;
using WorkoutTracker.API.Services.UserServices;
using Xunit;

namespace WorkoutTracker.API.Tests.Initializers;

public class RolesInitializer_Tests
{
    [Fact]
    public static async Task InitializeAsync()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);

        var roleRepository = new RoleRepository(roleManager);

        string userRoleStr = Roles.UserRole, adminRoleStr = Roles.AdminRole;

         //Act
        await RolesInitializer.InitializeAsync(roleRepository, userRoleStr, adminRoleStr);

        //Assert
        IdentityRole? userRole = await roleRepository.GetRoleByNameAsync(userRoleStr);

        Assert.NotNull(userRole);
        Assert.Equal(userRole?.Name, userRoleStr);

        IdentityRole? adminRole = await roleRepository.GetRoleByNameAsync(adminRoleStr);

        Assert.NotNull(adminRole);
        Assert.Equal(adminRole?.Name, adminRoleStr);

        var allRoles = (await roleRepository.GetRolesAsync()).ToList();

        Assert.NotNull(allRoles);
        Assert.Equal(allRoles?.Count(), 2);
        Assert.Contains(userRole, allRoles!);
        Assert.Contains(adminRole, allRoles!);
    }
}