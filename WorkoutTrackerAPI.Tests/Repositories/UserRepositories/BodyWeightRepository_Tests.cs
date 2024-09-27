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
    readonly WorkoutContextFactory contextFactory = new WorkoutContextFactory();

    async Task<string> InitializeUser(WorkoutDbContext db)
    {
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        string name = "User";
        var existingUser = await userRepository.GetUserByUsernameAsync(name);

        if (existingUser is null)
        {
            string email = "user@email.com";
            string password = "P@$$w0rd";

            await WorkoutContextFactory.InitializeRolesAsync(db);

            await UsersInitializer.InitializeAsync(userRepository, name, email, password, Roles.UserRole);

            return (await userRepository.GetUserIdByUsernameAsync(name))!;
        }

        return existingUser.Id;
    }

    [Fact]
    public async Task AddAsync_ShouldReturnNewBodyWeight()
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

        //Act
        var newBodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        //Assert
        Assert.NotNull(newBodyWeight);

        var bodyWeightById = await bodyWeightRepository.GetByIdAsync(newBodyWeight.Id);
        Assert.NotNull(bodyWeightById);

        Assert.Equal(newBodyWeight, bodyWeightById);
    }
    
    [Fact]
    public async Task AddAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        string userId = await InitializeUser(db);
        var bodyWeight = new BodyWeight()
        {
            Id = -1,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = userId
        };

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await bodyWeightRepository.AddAsync(bodyWeight));
        //var ex = await Assert.ThrowsAsync<Exception>(async () => await bodyWeightRepository.AddAsync(bodyWeight));
        Assert.Equal($"ID cannot be negative for entity of type {nameof(BodyWeight)}.", ex.Message);
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
