using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Repositories.UserRepositories;

public class BodyWeightRepository_Tests
{
    //readonly WorkoutContextFactory contextFactory = new WorkoutContextFactory();

    async Task<string> InitializeUser2(WorkoutDbContext db)
    {
        // create a IWebHost environment mock instance
        var mockEnv = Mock.Of<IWebHostEnvironment>();

        // create a IConfiguration mock instance
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "DefaultPasswords:RegisteredUser")]).Returns("M0ckP$$word");


        // create a RoleManager instance
        var roleManager = IdentityHelper.GetRoleManager(db);

        // create a UserManager instance
        var userManager = IdentityHelper.GetUserManager(db);

        // setup the default role names
        string role_RegisteredUser = "RegisteredUser";

        // create the default roles (if they don't exist yet)
        if (await roleManager.FindByNameAsync(role_RegisteredUser) ==null)
            await roleManager.CreateAsync(new Role(){ Name = role_RegisteredUser});


        // check if the standard user already exists
        var email_User = "user@email.com";
        if (await userManager.FindByNameAsync(email_User) == null)
        {
            // create a new standard ApplicationUser account
            var user_User = new User()
            {
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = email_User,
                Email = email_User
            };

            // insert the standard user into the DB
            await userManager.CreateAsync(user_User, mockConfiguration.Object["DefaultPasswords:RegisteredUser"]);

            // assign the "RegisteredUser" role
            await userManager.AddToRoleAsync(user_User,
             role_RegisteredUser);

            // confirm the e-mail and remove lockout
            user_User.EmailConfirmed = true;
            user_User.LockoutEnabled = false;

            return user_User.Id;
        }

        return string.Empty;
    }

    async Task<string> InitializeUser(WorkoutDbContext db)
    {
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        string name = "User";
        string email = "user@email.com";
        string password = "P@$$w0rd";

        var existingUser = await userRepository.GetUserByUsernameAsync(name);
        if (existingUser is null)
        {
            //await WorkoutContextFactory.InitializeRolesAsync(db);

            var roleManager = IdentityHelper.GetRoleManager(db);
            RoleRepository roleRepository = new(roleManager);
            //await RolesInitializer.InitializeAsync(roleRepository, Roles.UserRole, Roles.AdminRole);

            await roleRepository.AddRoleAsync(new Role() { Name = Roles.UserRole });
            await roleRepository.AddRoleAsync(new Role() { Name = Roles.AdminRole });

            //await UsersInitializer.InitializeAsync(userRepository, name, email, password, Roles.UserRole);
            User user = new()
            {
                UserName = name,
                Email = email,
                Registered = DateTime.Now
            };


            IdentityResult result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var res = await userManager.AddToRoleAsync(user, Roles.UserRole);
            }

            return (await userRepository.GetUserIdByUsernameAsync(name))!;
        }

        return existingUser.Id;
    }

    [Fact]
    public async Task AddAsync_ShouldReturnNewBodyWeight()
    {
        //Arrange

        WorkoutContextFactory contextFactory = new WorkoutContextFactory();
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser2(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };

        //Act
        var newBodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        //Assert
        Assert.NotNull(newBodyWeight);

        var bodyWeightById = await bodyWeightRepository.GetByIdAsync(newBodyWeight.Id);
        Assert.NotNull(bodyWeightById);

        Assert.Equal(newBodyWeight, bodyWeightById);
    }
    /*
    [Fact]
    public async Task AddAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeight = new BodyWeight()
        {
            Id = 7,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await bodyWeightRepository.AddAsync(bodyWeight));
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddBodyWeightsSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70.0f,
                    WeightType = WeightType.Kilogram,
                    UserId = userId
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = userId
                }
            };

        //Act
        await bodyWeightRepository.AddRangeAsync(bodyWeights);

        //Assert
        var addedBodyWeights = await bodyWeightRepository.GetAllAsync();
        Assert.NotNull(addedBodyWeights);
        Assert.Equal(2, addedBodyWeights.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Id = -4,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70.0f,
                    WeightType = WeightType.Kilogram,
                    UserId = userId
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = userId
                }
            };

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await bodyWeightRepository.AddRangeAsync(bodyWeights));
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveBodyWeightSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };
        var newBodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        //Act
        await bodyWeightRepository.RemoveAsync(newBodyWeight.Id);

        //Assert
        var bodyWeightById = await bodyWeightRepository.GetByIdAsync(newBodyWeight.Id);
        Assert.Null(bodyWeightById);

        var addedBodyWeights = await bodyWeightRepository.GetAllAsync();
        Assert.Empty(addedBodyWeights);
        Assert.Equal(0, addedBodyWeights.Count());
    }

    [Fact]
    public async Task RemoveAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await bodyWeightRepository.RemoveAsync(5));
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldRemoveBodyWeightsSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };
        var newBodyWeight1 = await bodyWeightRepository.AddAsync(bodyWeight);

        var bodyWeight2 = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
            Weight = 64,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };
        var newBodyWeight2 = await bodyWeightRepository.AddAsync(bodyWeight2);

        //Act
        await bodyWeightRepository.RemoveRangeAsync(new[] { newBodyWeight1, newBodyWeight2 });

        //Assert
        var bodyWeightById1 = await bodyWeightRepository.GetByIdAsync(newBodyWeight1.Id);
        Assert.Null(bodyWeightById1);

        var bodyWeightById2 = await bodyWeightRepository.GetByIdAsync(newBodyWeight1.Id);
        Assert.Null(bodyWeightById2);

        var bodyWeights = await bodyWeightRepository.GetAllAsync();
        Assert.Empty(bodyWeights);
        Assert.Equal(0, bodyWeights.Count());
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeight1 = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };

        var bodyWeight2 = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
            Weight = 64,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () =>
            await bodyWeightRepository.RemoveRangeAsync(new[] { bodyWeight1, bodyWeight2 }));
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnBodyWeights()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70.0f,
                    WeightType = WeightType.Kilogram,
                    UserId = userId
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = userId
                }
            };

        await bodyWeightRepository.AddRangeAsync(bodyWeights);

        //Act
        var addedBodyWeights = await bodyWeightRepository.GetAllAsync();

        //Assert
        Assert.NotNull(addedBodyWeights);
        Assert.Equal(2, addedBodyWeights.Count());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBodyWeightById()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };

        var newBodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        //Act
        var bodyWeightById = await bodyWeightRepository.GetByIdAsync(newBodyWeight.Id);

        //Assert
        Assert.NotNull(bodyWeightById);
        Assert.Equal(newBodyWeight.Id, bodyWeightById.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };

        var newBodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await bodyWeightRepository.GetByIdAsync(-1));
    }

    [Fact]
    public async Task FindAsync_ShouldFindBodyWeights()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = userId
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = userId
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = userId
                }
            };

        await bodyWeightRepository.AddRangeAsync(bodyWeights);

        //Act
        var someBodyWeights = await bodyWeightRepository.FindAsync(bw => bw.WeightType == WeightType.Kilogram && bw.Weight == 70);

        //Assert
        Assert.NotNull(someBodyWeights);
        Assert.Equal(2, someBodyWeights.Count());

        var result = someBodyWeights.All(bw => bw.WeightType == WeightType.Kilogram && bw.Weight == 70);
        Assert.True(result);
    }

    [Fact]
    public async Task FindAsync_ShouldFindNoBodyWeights()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = userId
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Kilogram,
                    UserId = userId
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = userId
                }
            };

        await bodyWeightRepository.AddRangeAsync(bodyWeights);

        //Act
        var someBodyWeights = await bodyWeightRepository.FindAsync(bw => bw.WeightType == WeightType.Pound && bw.Weight == 150);

        //Assert
        Assert.Empty(someBodyWeights);
    }
    */

    //[Fact]
    //public async Task ExistsAsync()
    //{

    //}

    //[Fact]
    //public async Task UpdateAsync()
    //{

    //}

    //[Fact]
    //public async Task SaveChangesAsync()
    //{

    //}
}
