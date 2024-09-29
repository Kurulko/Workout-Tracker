using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories;
using Xunit;
using WorkoutTrackerAPI.Initializers;

namespace WorkoutTrackerAPI.Tests.Repositories.UserRepositories;

public class MuscleSizeRepository_Tests : DbModelRepository_Tests<MuscleSize>
{
    async Task<Muscle> GetBackMuscleAsync(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);

        string name = "Back";
        var muscle = await muscleRepository.GetByNameAsync(name);

        if (muscle is null)
        {
            MuscleData muscleData = new()
            {
                Name = name
            };
            muscle = await MusclesInitializer.InitializeAsync(muscleRepository, muscleData, null);
        }

        return muscle;
    }

    async Task<Muscle> GetBicepsMuscleAsync(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);

        string name = "Biceps";
        var muscle = await muscleRepository.GetByNameAsync(name);

        if (muscle is null)
        {
            MuscleData muscleData = new()
            {
                Name = "Biceps"
            };
            muscle = await MusclesInitializer.InitializeAsync(muscleRepository, muscleData, null);
        }

        return muscle;
    }

    async Task<MuscleSize> GetValidMuscleSizeAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUser(db);
        Muscle muscle = await GetBicepsMuscleAsync(db);

        var validMuscleSize = new MuscleSize()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Size = 40,
            SizeType = SizeType.Centimeter,
            MuscleId = muscle.Id,
            UserId = user.Id
        };

        return validMuscleSize;
    }

    async Task<IEnumerable<MuscleSize>> GetValidMuscleSizesAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUser(db);
        Muscle bicepsMuscle = await GetBicepsMuscleAsync(db);
        Muscle backMuscle = await GetBackMuscleAsync(db);

        var validMuscleSizes = new[]
             {
                new MuscleSize()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Size = 40,
                    SizeType = SizeType.Centimeter,
                    MuscleId = bicepsMuscle.Id,
                    UserId = user.Id
                },
                new MuscleSize()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Size = 120,
                    SizeType = SizeType.Centimeter,
                    MuscleId = backMuscle.Id,
                    UserId = user.Id
                }
            };

        return validMuscleSizes;
    }

    async Task<MuscleSize> GetInvalidMuscleSizeAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUser(db);
        Muscle muscle = await GetBicepsMuscleAsync(db);

        var invalidMuscleSize = new MuscleSize()
        {
            Id = -1,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Size = 40,
            SizeType = SizeType.Centimeter,
            MuscleId = muscle.Id,
            UserId = user.Id
        };

        return invalidMuscleSize;
    }

    async Task<IEnumerable<MuscleSize>> GetInvalidMuscleSizesAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUser(db);
        Muscle bicepsMuscle = await GetBicepsMuscleAsync(db);
        Muscle backMuscle = await GetBackMuscleAsync(db);

        var invalidMuscleSizes = new[]
             {
                new MuscleSize()
                {
                    Id = -1,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Size = 40,
                    SizeType = SizeType.Centimeter,
                    MuscleId = bicepsMuscle.Id,
                    UserId = user.Id
                },
                new MuscleSize()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Size = 120,
                    SizeType = SizeType.Centimeter,
                    MuscleId = backMuscle.Id,
                    UserId = user.Id
                }
            };

        return invalidMuscleSizes;
    }

    [Fact]
    public async Task AddAsync_ShouldReturnNewMuscleSize()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);

        //Act & Assert
        await AddModel_ShouldReturnNewModel(muscleSizeRepository, validMuscleSize);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var invalidMuscleSize = await GetInvalidMuscleSizeAsync(db);

        //Act & Assert
        await AddModel_ShouldThrowException(muscleSizeRepository, invalidMuscleSize);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMuscleSizesSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSizes = await GetValidMuscleSizesAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldAddModelsSuccessfully(muscleSizeRepository, validMuscleSizes);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var invalidMuscleSizes = await GetInvalidMuscleSizesAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldThrowException(muscleSizeRepository, invalidMuscleSizes);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveMuscleSizeSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeRepository.AddAsync(validMuscleSize);

        //Act & Assert
        await RemoveModel_ShouldRemoveModelSuccessfully(muscleSizeRepository, validMuscleSize.Id);
    }

    [Fact]
    public async Task RemoveAsync_IncorrectMuscleSizeID_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        //Act & Assert
        await RemoveModel_IncorrectModelID_ShouldThrowException(muscleSizeRepository);
    }

    [Fact]
    public async Task RemoveAsync_MuscleSizeNotExist_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        //Act & Assert
        await RemoveModels_ModelNotExist_ShouldThrowException(muscleSizeRepository, 1);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldRemoveMuscleSizesSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSizes = await GetValidMuscleSizesAsync(db);
        await muscleSizeRepository.AddRangeAsync(validMuscleSizes);

        //Act & Assert
        await RemoveRangeModels_ShouldRemoveModelsSuccessfully(muscleSizeRepository, validMuscleSizes);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var invalidMuscleSizes = await GetInvalidMuscleSizesAsync(db);

        //Act & Assert
        await RemoveRangeModels_ShouldThrowException(muscleSizeRepository, invalidMuscleSizes);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMuscleSizes()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSizes = await GetValidMuscleSizesAsync(db);
        await muscleSizeRepository.AddRangeAsync(validMuscleSizes);

        //Act & Assert
        await GetAllModels_ShouldReturnModels(muscleSizeRepository, validMuscleSizes.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        //Act & Assert
        await GetAllModels_ShouldReturnEmpty(muscleSizeRepository);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMuscleSizeById()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeRepository.AddAsync(validMuscleSize);

        //Act & Assert
        await GetModelById_ShouldReturnModelById(muscleSizeRepository, validMuscleSize.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        //Act & Assert
        await GetModelById_ShouldReturnNull(muscleSizeRepository, 1);
    }

    [Fact]
    public async Task FindAsync_ShouldFindMuscleSizes()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        User user = await GetDefaultUser(db);
        Muscle bicepsMuscle = await GetBicepsMuscleAsync(db);
        Muscle backMuscle = await GetBackMuscleAsync(db);

        var validMuscleSizes = new[]
             {
                new MuscleSize()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Size = 40,
                    SizeType = SizeType.Centimeter,
                    MuscleId = bicepsMuscle.Id,
                    UserId = user.Id
                },
                new MuscleSize()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-100)),
                    Size = 35,
                    SizeType = SizeType.Centimeter,
                    MuscleId = bicepsMuscle.Id,
                    UserId = user.Id
                },
                new MuscleSize()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Size = 120,
                    SizeType = SizeType.Centimeter,
                    MuscleId = backMuscle.Id,
                    UserId = user.Id
                }
            };

        await muscleSizeRepository.AddRangeAsync(validMuscleSizes);

        //Act & Assert
        await FindModels_ShouldFindModels(muscleSizeRepository, m => m.SizeType == SizeType.Centimeter && m.MuscleId == bicepsMuscle.Id, 2);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        User user = await GetDefaultUser(db);
        Muscle bicepsMuscle = await GetBicepsMuscleAsync(db);
        Muscle backMuscle = await GetBackMuscleAsync(db);

        var validMuscleSizes = new[]
             {
                new MuscleSize()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Size = 40,
                    SizeType = SizeType.Centimeter,
                    MuscleId = bicepsMuscle.Id,
                    UserId = user.Id
                },
                new MuscleSize()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-150)),
                    Size = 35,
                    SizeType = SizeType.Centimeter,
                    MuscleId = bicepsMuscle.Id,
                    UserId = user.Id
                }
            };

        await muscleSizeRepository.AddRangeAsync(validMuscleSizes);

        //Act & Assert
        await FindModels_ShouldReturnEmpty(muscleSizeRepository, m => m.MuscleId == backMuscle.Id);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeRepository.AddAsync(validMuscleSize);

        //Act & Assert
        await ModelExists_ShouldReturnTrue(muscleSizeRepository, validMuscleSize.Id);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        //Act & Assert
        await ModelExists_ShouldReturnFalse(muscleSizeRepository, 1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeRepository.AddAsync(validMuscleSize);

        validMuscleSize.Size += 10;

        //Act & Assert
        await UpdateModel_ShouldUpdateSuccessfully(muscleSizeRepository, validMuscleSize);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeRepository.AddAsync(validMuscleSize);

        validMuscleSize.Id = -1;

        //Act & Assert
        await UpdateModel_ShouldThrowException(muscleSizeRepository, validMuscleSize);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesEntitySuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeRepository.AddAsync(validMuscleSize);

        validMuscleSize.Size += 10;

        //Act & Assert
        await SaveModelChanges_SaveEntitySuccessfully(muscleSizeRepository, validMuscleSize);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var muscleSizeRepository = new MuscleSizeRepository(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeRepository.AddAsync(validMuscleSize);

        validMuscleSize.Id = -1;

        //Act & Assert
        await SaveModelChanges_ShouldThrowException(muscleSizeRepository);
    }
}
