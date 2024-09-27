using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.RoleServices;
using WorkoutTrackerAPI.Services.UserServices;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Initializers;

public class RolesInitializer_Tests
{
    [Fact]
    public static async Task InitializeAsync()
    {
        //Arrange
        WorkoutContextFactory factory = new();
        using var db = factory.CreateDatabaseContext();
        var roleManager = IdentityHelper.GetRoleManager(db);

        var roleRepository = new RoleRepository(roleManager);

        string userRoleStr = Roles.UserRole, adminRoleStr = Roles.AdminRole;

         //Act
        await RolesInitializer.InitializeAsync(roleRepository, userRoleStr, adminRoleStr);

        //Assert
        Role? userRole = await roleRepository.GetRoleByNameAsync(userRoleStr);

        Assert.NotNull(userRole);
        Assert.Equal(userRole?.Name, userRoleStr);

        Role? adminRole = await roleRepository.GetRoleByNameAsync(adminRoleStr);

        Assert.NotNull(adminRole);
        Assert.Equal(adminRole?.Name, adminRoleStr);

        var allRoles = (await roleRepository.GetRolesAsync()).ToList();

        Assert.NotNull(allRoles);
        Assert.Equal(allRoles?.Count(), 2);
        Assert.Contains(userRole, allRoles!);
        Assert.Contains(adminRole, allRoles!);
    }
}