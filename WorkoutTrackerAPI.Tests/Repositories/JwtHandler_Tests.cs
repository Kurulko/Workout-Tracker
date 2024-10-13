using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Settings;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Repositories;

public class JwtHandler_Tests
{
    readonly JwtSettings jwtSettings = new()
    {
        Audience = "Kurulko's audience",
        Issuer = "KurulkoServer",
        SecretKey = Guid.NewGuid().ToString(),
        ExpirationDays = 5
    };

    User GetValidUser()
    {
        var validUser = new User()
        {
            UserName = "Kurulko",
            Email = "kurulko@gmail.com",
            Registered = DateTime.Now
        };

        return validUser;
    }

    [Fact]
    public async Task GenerateJwtToken_ShouldGenerateJwtTokenSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var userRoles = new[] { Roles.AdminRole, Roles.UserRole };
        await userRepository.AddRolesToUserAsync(user.Id, userRoles);

        //Act
        var tokenModel = await jwtHandler.GenerateJwtTokenAsync(user);

        //Assert
        Assert.NotNull(tokenModel);
        Assert.False(string.IsNullOrEmpty(tokenModel.TokenStr));
        Assert.Equal(jwtSettings.ExpirationDays, tokenModel.ExpirationDays);
        Assert.Equal(userRoles, tokenModel.Roles);
    }

    [Fact]
    public async Task GetPrincipalFromToken_ShouldReturnPrincipalFromToken()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var userRoles = new[] { Roles.AdminRole, Roles.UserRole };
        await userRepository.AddRolesToUserAsync(user.Id, userRoles);

        var tokenModel = await jwtHandler.GenerateJwtTokenAsync(user);

        //Act
        var claims = jwtHandler.GetPrincipalFromToken(tokenModel.TokenStr);

        //Assert
        Assert.NotNull(claims);

        var userName = claims.Identity?.Name;
        Assert.Equal(user.UserName, userName);
    }

    [Fact]
    public void GetPrincipalFromToken_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var jwtHandler = new JwtHandler(jwtSettings, userManager);

        //Act & Assert
        var ex = Assert.ThrowsAny<Exception>(() => jwtHandler.GetPrincipalFromToken(Guid.NewGuid().ToString()));
    }
}
