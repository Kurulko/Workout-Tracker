using Microsoft.Extensions.Configuration;
using Moq;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data.Models.UserModels;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories.UserRepositories;
using Xunit;

namespace WorkoutTracker.API.Tests.Initializers;

public class UsersInitializer_Tests
{
    [Fact]
    public async Task InitializeAsync()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "DefaultPasswords:User")]).Returns("P@$$w0rd_2024");
        mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "DefaultPasswords:Admin")]).Returns("P@$$w0rd_2024");

        string name_User = "User";
        string name_Admin = "Admin";

        string email_User = "user@email.com";
        string email_Admin = "admin@email.com";

        string password_User = mockConfiguration.Object["DefaultPasswords:User"]!;
        string password_Admin = mockConfiguration.Object["DefaultPasswords:Admin"]!;

        await WorkoutContextFactory.InitializeRolesAsync(db);

        //Act
        await UsersInitializer.InitializeAsync(userRepository, name_User, email_User, password_User, new[] { Roles.UserRole });
        await UsersInitializer.InitializeAsync(userRepository, name_Admin, email_Admin, password_Admin, new[] { Roles.UserRole, Roles.AdminRole });

        //Assert
        User? user = await userRepository.GetUserByUsernameAsync(name_User);

        Assert.NotNull(user);
        Assert.Equal(user?.UserName, name_User);
        Assert.Equal(user?.Email, email_User);

        var userRoles = await userRepository.GetUserRolesAsync(user?.Id!);
        Assert.Contains(Roles.UserRole, userRoles!);
        Assert.Equal(1, userRoles?.Count()!);

        //var hasUserPassword = await userRepository.HasUserPasswordAsync(user?.Id!);
        //Assert.True(hasUserPassword);

        User? admin = await userRepository.GetUserByUsernameAsync(name_Admin)!;

        Assert.NotNull(admin);
        Assert.Equal(admin?.UserName, name_Admin);
        Assert.Equal(admin?.Email, email_Admin);

        var adminRoles = await userRepository.GetUserRolesAsync(admin?.Id!);
        Assert.Contains(Roles.UserRole, adminRoles!);
        Assert.Contains(Roles.AdminRole, adminRoles!);
        Assert.Equal(2, adminRoles?.Count()!);

        //var hasAdminPassword = await userRepository.HasUserPasswordAsync(admin?.Id!);
        //Assert.True(hasAdminPassword);
    }
}