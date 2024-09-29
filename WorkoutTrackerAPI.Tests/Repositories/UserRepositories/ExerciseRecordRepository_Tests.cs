using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Repositories.UserRepositories;

public class ExerciseRecordRepository_Tests : DbModelRepository_Tests<ExerciseRecord>
{
    async Task<Exercise> GetPullUpExerciseAsync(WorkoutDbContext db)
    {
        var exerciseRepository = new ExerciseRepository(db);

        string name = "Pull Up";
        var exercise = await exerciseRepository.GetByNameAsync(name);

        if (exercise is null)
        {
            var muscleRepository = new MuscleRepository(db);
            exercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, name, ExerciseType.Reps,
                "Latissimus dorsi", "Biceps brachii", "Teres minor");
        }

        return exercise;
    }

    async Task<Exercise> GetPlankExerciseAsync(WorkoutDbContext db)
    {
        var exerciseRepository = new ExerciseRepository(db);

        string name = "Plank";
        var exercise = await exerciseRepository.GetByNameAsync(name);

        if (exercise is null)
        {
            var muscleRepository = new MuscleRepository(db);
            exercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, name, ExerciseType.Time,
            "Rectus abdominis", "External oblique", "Quadriceps");
        }

        return exercise;
    }


    async Task<ExerciseRecord> GetValidExerciseRecordAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExerciseAsync(db);

        var validExerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        return validExerciseRecord;
    }

    async Task<IEnumerable<ExerciseRecord>> GetValidExerciseRecordsAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUser(db);
        Exercise exercise1 = await GetPullUpExerciseAsync(db);
        Exercise exercise2 = await GetPlankExerciseAsync(db);

        var validExerciseRecords = new[]
             {
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Reps = 20,
                    SumOfReps = 20,
                    CountOfTimes = 1,
                    UserId = user.Id,
                    ExerciseId = exercise1.Id
                },
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Time = new TimeSpan(0, 1, 0),
                    SumOfTime= new TimeSpan(0, 1, 0),
                    CountOfTimes = 1,
                    UserId = user.Id,
                    ExerciseId = exercise2.Id
                }
            };

        return validExerciseRecords;
    }

    async Task<ExerciseRecord> GetInvalidExerciseRecordAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExerciseAsync(db);

        var invalidExerciseRecord = new ExerciseRecord()
        {
            Id = -1,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        return invalidExerciseRecord;
    }

    async Task<IEnumerable<ExerciseRecord>> GetInvalidExerciseRecordsAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUser(db);
        Exercise exercise1 = await GetPullUpExerciseAsync(db);
        Exercise exercise2 = await GetPlankExerciseAsync(db);

        var invalidExerciseRecords = new[]
             {
                new ExerciseRecord()
                {
                    Id = -1,
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Reps = 20,
                    SumOfReps = 20,
                    CountOfTimes = 1,
                    UserId = user.Id,
                    ExerciseId = exercise1.Id
                },
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Time = new TimeSpan(0, 1, 0),
                    SumOfTime= new TimeSpan(0, 1, 0),
                    CountOfTimes = 1,
                    UserId = user.Id,
                    ExerciseId = exercise2.Id
                }
            };

        return invalidExerciseRecords;
    }


    [Fact]
    public async Task AddAsync_ShouldReturnNewExerciseRecord()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);

        //Act & Assert
        await AddModel_ShouldReturnNewModel(exerciseRecordRepository, validExerciseRecord);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var invalidExerciseRecord = await GetInvalidExerciseRecordAsync(db);

        //Act & Assert
        await AddModel_ShouldThrowException(exerciseRecordRepository, invalidExerciseRecord);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddExerciseRecordsSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecords = await GetValidExerciseRecordsAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldAddModelsSuccessfully(exerciseRecordRepository, validExerciseRecords);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var invalidExerciseRecords = await GetInvalidExerciseRecordsAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldThrowException(exerciseRecordRepository, invalidExerciseRecords);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveExerciseRecordSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordRepository.AddAsync(validExerciseRecord);

        //Act & Assert
        await RemoveModel_ShouldRemoveModelSuccessfully(exerciseRecordRepository, validExerciseRecord.Id);
    }

    [Fact]
    public async Task RemoveAsync_IncorrectExerciseRecordID_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act & Assert
        await RemoveModel_IncorrectModelID_ShouldThrowException(exerciseRecordRepository);
    }

    [Fact]
    public async Task RemoveAsync_ExerciseRecordNotExist_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act & Assert
        await RemoveModels_ModelNotExist_ShouldThrowException(exerciseRecordRepository, 1);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldRemoveExerciseRecordsSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecords = await GetValidExerciseRecordsAsync(db);
        await exerciseRecordRepository.AddRangeAsync(validExerciseRecords);

        //Act & Assert
        await RemoveRangeModels_ShouldRemoveModelsSuccessfully(exerciseRecordRepository, validExerciseRecords);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var invalidExerciseRecords = await GetInvalidExerciseRecordsAsync(db);

        //Act & Assert
        await RemoveRangeModels_ShouldThrowException(exerciseRecordRepository, invalidExerciseRecords);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnExerciseRecords()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecords = await GetValidExerciseRecordsAsync(db);
        await exerciseRecordRepository.AddRangeAsync(validExerciseRecords);

        //Act & Assert
        await GetAllModels_ShouldReturnModels(exerciseRecordRepository, validExerciseRecords.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act & Assert
        await GetAllModels_ShouldReturnEmpty(exerciseRecordRepository);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnExerciseRecordById()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordRepository.AddAsync(validExerciseRecord);

        //Act & Assert
        await GetModelById_ShouldReturnModelById(exerciseRecordRepository, validExerciseRecord.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act & Assert
        await GetModelById_ShouldReturnNull(exerciseRecordRepository, 1);
    }

    [Fact]
    public async Task FindAsync_ShouldFindExerciseRecords()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise1 = await GetPullUpExerciseAsync(db);
        Exercise exercise2 = await GetPlankExerciseAsync(db);

        var exerciseRecords = new[]
             {
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Reps = 20,
                    SumOfReps = 20,
                    CountOfTimes = 1,
                    UserId = user.Id,
                    ExerciseId = exercise1.Id
                },
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Time = new TimeSpan(0, 1, 0),
                    SumOfTime= new TimeSpan(0, 1, 0),
                    CountOfTimes = 1,
                    UserId = user.Id,
                    ExerciseId = exercise2.Id
                }
            };

        await exerciseRecordRepository.AddRangeAsync(exerciseRecords);

        //Act & Assert
        await FindModels_ShouldFindModels(exerciseRecordRepository, er => er.ExerciseId == exercise2.Id && er.UserId == user.Id, 1);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise1 = await GetPullUpExerciseAsync(db);
        Exercise exercise2 = await GetPlankExerciseAsync(db);

        var validExerciseRecords = new[]
             {
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Reps = 20,
                    SumOfReps = 20,
                    CountOfTimes = 1,
                    UserId = user.Id,
                    ExerciseId = exercise1.Id
                },
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Time = new TimeSpan(0, 1, 0),
                    SumOfTime= new TimeSpan(0, 1, 0),
                    CountOfTimes = 1,
                    UserId = user.Id,
                    ExerciseId = exercise2.Id
                }
            };

        await exerciseRecordRepository.AddRangeAsync(validExerciseRecords);

        //Act & Assert
        await FindModels_ShouldReturnEmpty(exerciseRecordRepository, er => er.ExerciseId == exercise2.Id && er.UserId != user.Id);
    }


    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordRepository.AddAsync(validExerciseRecord);

        //Act & Assert
        await ModelExists_ShouldReturnTrue(exerciseRecordRepository, validExerciseRecord.Id);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act & Assert
        await ModelExists_ShouldReturnFalse(exerciseRecordRepository, 1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordRepository.AddAsync(validExerciseRecord);

        validExerciseRecord.CountOfTimes++; ;
        validExerciseRecord.SumOfReps += 10;

        //Act & Assert
        await UpdateModel_ShouldUpdateSuccessfully(exerciseRecordRepository, validExerciseRecord);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordRepository.AddAsync(validExerciseRecord);

        validExerciseRecord.Id = -1;

        //Act & Assert
        await UpdateModel_ShouldThrowException(exerciseRecordRepository, validExerciseRecord);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesEntitySuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordRepository.AddAsync(validExerciseRecord);

        validExerciseRecord.CountOfTimes++; ;
        validExerciseRecord.SumOfReps += 10;

        //Act & Assert
        await SaveModelChanges_SaveEntitySuccessfully(exerciseRecordRepository, validExerciseRecord);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordRepository.AddAsync(validExerciseRecord);

        validExerciseRecord.Id = -1;

        //Act & Assert
        await SaveModelChanges_ShouldThrowException(exerciseRecordRepository);
    }
}
