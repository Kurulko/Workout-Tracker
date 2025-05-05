using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Data.Models.UserModels;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Repositories;
using Xunit;

namespace WorkoutTracker.API.Tests.Repositories.WorkoutRepositories;

public class MuscleRepository_Tests : BaseWorkoutRepository_Tests<Muscle>
{
    static Muscle GetValidMuscle()
    {
        var validMuscle = new Muscle()
        {
           Name = "Back"
        };

        return validMuscle;
    }

    static IEnumerable<Muscle> GetValidMuscles()
    {
        var validMuscles = new[]
             {
                new Muscle()
                {
                    Name = "Back"
                },
                new Muscle()
                {
                    Name = "Biceps"
                },
                new Muscle()
                {
                    Name = "Calves"
                }
            };

        return validMuscles;
    }

    Muscle GetInvalidMuscle()
    {
        var invalidMuscle = new Muscle()
        {
            Id = -1,
            Name = "Back"
        };

        return invalidMuscle;
    }

    IEnumerable<Muscle> GetInvalidMuscles()
    {
        var invalidMuscles = new[]
             {
                new Muscle()
                {
                    Id = -1,
                    Name = "Back"
                },
                new Muscle()
                {
                    Name = "Biceps"
                },
                new Muscle()
                {
                    Name = "Calves"
                }
            };

        return invalidMuscles;
    }

    [Fact]
    public async Task AddAsync_ShouldReturnNewMuscle()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();

        //Act & Assert
        await AddModel_ShouldReturnNewModel(muscleRepository, validMuscle);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var invalidMuscle = GetInvalidMuscle();

        //Act & Assert
        await AddModel_ShouldThrowException(muscleRepository, invalidMuscle);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddMusclesSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscles = GetValidMuscles();

        //Act & Assert
        await AddRangeModels_ShouldAddModelsSuccessfully(muscleRepository, validMuscles);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var invalidMuscles = GetInvalidMuscles();

        //Act & Assert
        await AddRangeModels_ShouldThrowException(muscleRepository, invalidMuscles);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveMuscleSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();
        await muscleRepository.AddAsync(validMuscle);

        //Act & Assert
        await RemoveModel_ShouldRemoveModelSuccessfully(muscleRepository, validMuscle.Id);
    }

    [Fact]
    public async Task RemoveAsync_IncorrectMuscleID_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        //Act & Assert
        await RemoveModel_IncorrectModelID_ShouldThrowException(muscleRepository);
    }

    [Fact]
    public async Task RemoveAsync_MuscleNotExist_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        //Act & Assert
        await RemoveModels_ModelNotExist_ShouldThrowException(muscleRepository, 1);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldRemoveMusclesSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscles = GetValidMuscles();
        await muscleRepository.AddRangeAsync(validMuscles);

        //Act & Assert
        await RemoveRangeModels_ShouldRemoveModelsSuccessfully(muscleRepository, validMuscles);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var invalidMuscles = GetInvalidMuscles();

        //Act & Assert
        await RemoveRangeModels_ShouldThrowException(muscleRepository, invalidMuscles);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnMuscles()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscles = GetValidMuscles();
        await muscleRepository.AddRangeAsync(validMuscles);

        //Act & Assert
        await GetAllModels_ShouldReturnModels(muscleRepository, validMuscles.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        //Act & Assert
        await GetAllModels_ShouldReturnEmpty(muscleRepository);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnMuscleById()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();
        await muscleRepository.AddAsync(validMuscle);

        //Act & Assert
        await GetModelById_ShouldReturnModelById(muscleRepository, validMuscle.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        //Act & Assert
        await GetModelById_ShouldReturnNull(muscleRepository, 1);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnMuscleByName()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();
        await muscleRepository.AddAsync(validMuscle);

        //Act & Assert
        await GetModelByName_ShouldReturnModelByName(muscleRepository, validMuscle.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        //Act & Assert
        await GetModelByName_ShouldReturnNull(muscleRepository, "Non-existent muscle");
    }

    [Fact]
    public async Task FindAsync_ShouldFindMuscles()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscles = new[]
             {
                new Muscle()
                {
                    Name = "Back"
                },
                new Muscle()
                {
                    Name = "Biceps"
                },
                new Muscle()
                {
                    Name = "Calves"
                }
            };

        await muscleRepository.AddRangeAsync(validMuscles);

        //Act & Assert
        await FindModels_ShouldFindModels(muscleRepository, m => m.Name == "Biceps", 1);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscles = new[]
             {
                new Muscle()
                {
                    Name = "Back"
                },
                new Muscle()
                {
                    Name = "Biceps"
                },
                new Muscle()
                {
                    Name = "Calves"
                }
            };

        await muscleRepository.AddRangeAsync(validMuscles);

        //Act & Assert
        await FindModels_ShouldReturnEmpty(muscleRepository, m => m.Name == "Legs");
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();
        await muscleRepository.AddAsync(validMuscle);

        //Act & Assert
        await ModelExists_ShouldReturnTrue(muscleRepository, validMuscle.Id);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        //Act & Assert
        await ModelExists_ShouldReturnFalse(muscleRepository, 1);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();
        await muscleRepository.AddAsync(validMuscle);

        //Act & Assert
        await ModelExistsByName_ShouldReturnTrue(muscleRepository, validMuscle.Name);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        //Act & Assert
        await ModelExistsByName_ShouldReturnFalse(muscleRepository, "Non-existent muscle");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();
        await muscleRepository.AddAsync(validMuscle);

        validMuscle.Name = "New Name";

        //Act & Assert
        await UpdateModel_ShouldUpdateSuccessfully(muscleRepository, validMuscle);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();
        await muscleRepository.AddAsync(validMuscle);

        validMuscle.Id = -1;

        //Act & Assert
        await UpdateModel_ShouldThrowException(muscleRepository, validMuscle);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesEntitySuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();
        await muscleRepository.AddAsync(validMuscle);

        validMuscle.Name = "New Name";

        //Act & Assert
        await SaveModelChanges_SaveEntitySuccessfully(muscleRepository, validMuscle);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        var validMuscle = GetValidMuscle();
        await muscleRepository.AddAsync(validMuscle);

        validMuscle.Id = -1;

        //Act & Assert
        await SaveModelChanges_ShouldThrowException(muscleRepository);
    }
}