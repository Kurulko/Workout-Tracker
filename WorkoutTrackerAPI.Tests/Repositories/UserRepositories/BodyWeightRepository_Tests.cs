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

public class BodyWeightRepository_Tests : DbModelRepository_Tests<BodyWeight>
{
    async Task<BodyWeight> GetValidBodyWeightAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);

        var validBodyWeight = new BodyWeight()
        {
            Date = DateTime.Now,
            Weight = 70,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        return validBodyWeight;
    }

    async Task<IEnumerable<BodyWeight>> GetValidBodyWeightsAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);

        var validBodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateTime.Now,
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateTime.Now.AddDays(-2),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
                }
            };

        return validBodyWeights;
    }

    async Task<BodyWeight> GetInvalidBodyWeightAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);

        var invalidBodyWeight = new BodyWeight()
        {
            Id = -1,
            Date = DateTime.Now,
            Weight = 70,
            WeightType = WeightType.Kilogram,
            UserId = user.Id
        };

        return invalidBodyWeight;
    }

    async Task<IEnumerable<BodyWeight>> GetInvalidBodyWeightsAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);

        var invalidBodyWeights = new[]
             {
                new BodyWeight()
                {
                    Id = -1,
                    Date = DateTime.Now,
                    Weight = 70.0f,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateTime.Now.AddDays(-2),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
                }
            };

        return invalidBodyWeights;
    }

    [Fact]
    public async Task AddAsync_ShouldReturnNewBodyWeight()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeight = await GetValidBodyWeightAsync(db);

        //Act & Assert
        await AddModel_ShouldReturnNewModel(bodyWeightRepository, validBodyWeight);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var invalidBodyWeight = await GetInvalidBodyWeightAsync(db);

        //Act & Assert
        await AddModel_ShouldThrowException(bodyWeightRepository, invalidBodyWeight);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddBodyWeightsSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeights = await GetValidBodyWeightsAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldAddModelsSuccessfully(bodyWeightRepository, validBodyWeights);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var invalidBodyWeights = await GetInvalidBodyWeightsAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldThrowException(bodyWeightRepository, invalidBodyWeights);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveBodyWeightSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeight = await GetValidBodyWeightAsync(db);
        await bodyWeightRepository.AddAsync(validBodyWeight);

        //Act & Assert
        await RemoveModel_ShouldRemoveModelSuccessfully(bodyWeightRepository, validBodyWeight.Id);
    }

    [Fact]
    public async Task RemoveAsync_IncorrectBodyWeightID_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act & Assert
        await RemoveModel_IncorrectModelID_ShouldThrowException(bodyWeightRepository);
    }

    [Fact]
    public async Task RemoveAsync_BodyWeightNotExist_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act & Assert
        await RemoveModels_ModelNotExist_ShouldThrowException(bodyWeightRepository, 1);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldRemoveBodyWeightsSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeights = await GetValidBodyWeightsAsync(db);
        await bodyWeightRepository.AddRangeAsync(validBodyWeights);

        //Act & Assert
        await RemoveRangeModels_ShouldRemoveModelsSuccessfully(bodyWeightRepository, validBodyWeights);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var invalidBodyWeights = await GetInvalidBodyWeightsAsync(db);

        //Act & Assert
        await RemoveRangeModels_ShouldThrowException(bodyWeightRepository, invalidBodyWeights);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnBodyWeights()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeights = await GetValidBodyWeightsAsync(db);
        await bodyWeightRepository.AddRangeAsync(validBodyWeights);

        //Act & Assert
        await GetAllModels_ShouldReturnModels(bodyWeightRepository, validBodyWeights.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act & Assert
        await GetAllModels_ShouldReturnEmpty(bodyWeightRepository);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnBodyWeightById()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeight = await GetValidBodyWeightAsync(db);
        await bodyWeightRepository.AddAsync(validBodyWeight);

        //Act & Assert
        await GetModelById_ShouldReturnModelById(bodyWeightRepository, validBodyWeight.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act & Assert
        await GetModelById_ShouldReturnNull(bodyWeightRepository, 1);
    }

    [Fact]
    public async Task FindAsync_ShouldFindBodyWeights()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUserAsync(db);
        var validBodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateTime.Now,
                    Weight = 70.0f,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateTime.Now.AddDays(-2),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateTime.Now.AddDays(-300),
                    Weight = 254,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
                }
            };

        await bodyWeightRepository.AddRangeAsync(validBodyWeights);

        //Act & Assert
        await FindModels_ShouldFindModels(bodyWeightRepository, bw => bw.WeightType == WeightType.Pound, 2);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        User user = await GetDefaultUserAsync(db);

        var validBodyWeights = new[]
             {
                new BodyWeight()
                {
                    Date = DateTime.Now,
                    Weight = 70.0f,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateTime.Now.AddDays(-2),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateTime.Now.AddDays(-300),
                    Weight = 254,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
                }
            };

        await bodyWeightRepository.AddRangeAsync(validBodyWeights);

        //Act & Assert
        await FindModels_ShouldReturnEmpty(bodyWeightRepository, er => er.WeightType == WeightType.Kilogram && er.Weight == 1000);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeight = await GetValidBodyWeightAsync(db);
        await bodyWeightRepository.AddAsync(validBodyWeight);

        //Act & Assert
        await ModelExists_ShouldReturnTrue(bodyWeightRepository, validBodyWeight.Id);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        //Act & Assert
        await ModelExists_ShouldReturnFalse(bodyWeightRepository, 1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeight = await GetValidBodyWeightAsync(db);
        await bodyWeightRepository.AddAsync(validBodyWeight);

        validBodyWeight.Weight += 10;

        //Act & Assert
        await UpdateModel_ShouldUpdateSuccessfully(bodyWeightRepository, validBodyWeight);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeight = await GetValidBodyWeightAsync(db);
        await bodyWeightRepository.AddAsync(validBodyWeight);

        validBodyWeight.Id = -1;

        //Act & Assert
        await UpdateModel_ShouldThrowException(bodyWeightRepository, validBodyWeight);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesEntitySuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeight = await GetValidBodyWeightAsync(db);
        await bodyWeightRepository.AddAsync(validBodyWeight);

        validBodyWeight.Weight += 10;

        //Act & Assert
        await SaveModelChanges_SaveEntitySuccessfully(bodyWeightRepository, validBodyWeight);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepository = new BodyWeightRepository(db);

        var validBodyWeight = await GetValidBodyWeightAsync(db);
        await bodyWeightRepository.AddAsync(validBodyWeight);

        validBodyWeight.Id = -1;

        //Act & Assert
        await SaveModelChanges_ShouldThrowException(bodyWeightRepository);
    }
}
