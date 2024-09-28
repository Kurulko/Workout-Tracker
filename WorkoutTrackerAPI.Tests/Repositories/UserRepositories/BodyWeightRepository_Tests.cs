using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Repositories.UserRepositories;

public class BodyWeightRepository_Tests : BaseRepository_Tests<BodyWeight>
{
    [Fact]
    public async Task AddAsync_ShouldReturnNewBodyWeight()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
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

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Id = -1,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await bodyWeightRepository.AddAsync(bodyWeight));
        Assert.Equal($"Entity of type {nameof(BodyWeight)} should not have an ID assigned.", ex.Message);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddBodyWeightsSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70.0f,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
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

        User user = await GetDefaultUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Id = -4,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70.0f,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
                }
            };

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await bodyWeightRepository.AddRangeAsync(bodyWeights));
        Assert.Equal($"New entities of type {nameof(BodyWeight)} should not have an ID assigned.", ex.Message);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveBodyWeightSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
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
    public async Task RemoveAsync_IncorrectBodyWeightID_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await bodyWeightRepository.RemoveAsync(-1));
        Assert.Equal($"Entity of type {nameof(BodyWeight)} must have a positive ID to be removed.", ex.Message);
    }

    [Fact]
    public async Task RemoveAsync_BodyWeightNotExist_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await bodyWeightRepository.RemoveAsync(2));
        Assert.Equal($"{nameof(BodyWeight)} not found.", ex.Message);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldRemoveBodyWeightsSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };
        var newBodyWeight1 = await bodyWeightRepository.AddAsync(bodyWeight);

        var bodyWeight2 = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
            Weight = 64,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
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

        User user = await GetDefaultUser(db);
        var bodyWeight1 = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        var bodyWeight2 = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-5)),
            Weight = 64,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await bodyWeightRepository.RemoveRangeAsync(new[] { bodyWeight1, bodyWeight2 }));
        Assert.Equal($"Entities of type {nameof(BodyWeight)} must have a positive ID to be removed.", ex.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnBodyWeights()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70.0f,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
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
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act
        var addedBodyWeights = await bodyWeightRepository.GetAllAsync();

        //Assert
        Assert.NotNull(addedBodyWeights);
        Assert.Empty(addedBodyWeights);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBodyWeightById()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        var newBodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        //Act
        var bodyWeightById = await bodyWeightRepository.GetByIdAsync(newBodyWeight.Id);

        //Assert
        Assert.NotNull(bodyWeightById);
        Assert.Equal(newBodyWeight.Id, bodyWeightById.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act
        var bodyWeightById = await bodyWeightRepository.GetByIdAsync(1);

        //Assert
        Assert.Null(bodyWeightById);
    }

    [Fact]
    public async Task FindAsync_ShouldFindBodyWeights()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
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
    public async Task FindAsync_ShouldReturnEmpty()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-2)),
                    Weight = 62,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-1)),
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                }
            };

        await bodyWeightRepository.AddRangeAsync(bodyWeights);

        //Act
        var someBodyWeights = await bodyWeightRepository.FindAsync(bw => bw.WeightType == WeightType.Pound && bw.Weight == 150);

        //Assert
        Assert.Empty(someBodyWeights);
    }


    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        var newBodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        //Act
        var exists = await bodyWeightRepository.ExistsAsync(newBodyWeight.Id);

        //Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act
        var exists = await bodyWeightRepository.ExistsAsync(1);

        //Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfull()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        bodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        bodyWeight.WeightType = WeightType.Pound;
        bodyWeight.Weight = 120;

        //Act
        await bodyWeightRepository.UpdateAsync(bodyWeight);

        //Assert
        var updatedBodyWeight = await bodyWeightRepository.GetByIdAsync(bodyWeight.Id);

        Assert.NotNull(updatedBodyWeight);
        Assert.Equal(updatedBodyWeight, bodyWeight);
        Assert.Equal(updatedBodyWeight.Weight, bodyWeight.Weight);
        Assert.Equal(updatedBodyWeight.WeightType, bodyWeight.WeightType);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        bodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        bodyWeight.Id = -1;
        bodyWeight.Weight = 120;

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await bodyWeightRepository.UpdateAsync(bodyWeight));
        Assert.Equal($"Modified entities of type {nameof(BodyWeight)} must have a positive ID.", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesEntitySuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        bodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        var bodyWeightById = (await bodyWeightRepository.GetByIdAsync(bodyWeight.Id))!;

        bodyWeightById.WeightType = WeightType.Pound;
        bodyWeightById.Weight = 120;

        //Act
        await bodyWeightRepository.SaveChangesAsync();

        //Assert

        var updatedBodyWeight = (await bodyWeightRepository.GetByIdAsync(bodyWeight.Id))!;
        Assert.NotNull(updatedBodyWeight);
        Assert.Equal(updatedBodyWeight, bodyWeightById);
        Assert.Equal(updatedBodyWeight.Weight, bodyWeightById.Weight);
        Assert.Equal(updatedBodyWeight.WeightType, bodyWeightById.WeightType);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUser(db);
        var bodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70.0f,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        bodyWeight = await bodyWeightRepository.AddAsync(bodyWeight);

        var bodyWeightById = (await bodyWeightRepository.GetByIdAsync(bodyWeight.Id))!;

        bodyWeightById.Id = -1;

        //Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(bodyWeightRepository.SaveChangesAsync);
    }
}
