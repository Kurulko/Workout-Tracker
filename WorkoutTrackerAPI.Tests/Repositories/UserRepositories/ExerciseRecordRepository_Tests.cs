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

public class ExerciseRecordRepository_Tests : BaseRepository_Tests<ExerciseRecord>
{
    async Task<Exercise> GetPullUpExercise(WorkoutDbContext db)
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

    async Task<Exercise> GetPlankExercise(WorkoutDbContext db)
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

    [Fact]
    public async Task AddAsync_ShouldReturnNewExerciseRecord()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        //Act
        var newExerciseRecord = await exerciseRecordRepository.AddAsync(exerciseRecord);

        //Assert
        Assert.NotNull(newExerciseRecord);

        var exerciseRecordById = await exerciseRecordRepository.GetByIdAsync(newExerciseRecord.Id);
        Assert.NotNull(exerciseRecordById);

        Assert.Equal(newExerciseRecord, exerciseRecordById);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Id = -1,
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await exerciseRecordRepository.AddAsync(exerciseRecord));
        Assert.Equal($"Entity of type {nameof(ExerciseRecord)} should not have an ID assigned.", ex.Message);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddExerciseRecordsSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise1 = await GetPullUpExercise(db);
        Exercise exercise2 = await GetPlankExercise(db);
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

        //Act
        await exerciseRecordRepository.AddRangeAsync(exerciseRecords);

        //Assert
        var addedExerciseRecords = await exerciseRecordRepository.GetAllAsync();
        Assert.NotNull(addedExerciseRecords);
        Assert.Equal(2, addedExerciseRecords.Count());
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise1 = await GetPullUpExercise(db);
        Exercise exercise2 = await GetPlankExercise(db);
        var exerciseRecords = new[]
             {
                new ExerciseRecord()
                {
                    Id = -2,
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

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await exerciseRecordRepository.AddRangeAsync(exerciseRecords));
        Assert.Equal($"New entities of type {nameof(ExerciseRecord)} should not have an ID assigned.", ex.Message);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveExerciseRecordSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        var newExerciseRecord = await exerciseRecordRepository.AddAsync(exerciseRecord);

        //Act
        await exerciseRecordRepository.RemoveAsync(newExerciseRecord.Id);

        //Assert
        var exerciseRecordById = await exerciseRecordRepository.GetByIdAsync(newExerciseRecord.Id);
        Assert.Null(exerciseRecordById);

        var addedExerciseRecords = await exerciseRecordRepository.GetAllAsync();
        Assert.Empty(addedExerciseRecords);
        Assert.Equal(0, addedExerciseRecords.Count());
    }

    [Fact]
    public async Task RemoveAsync_IncorrectExerciseRecordID_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await exerciseRecordRepository.RemoveAsync(-1));
        Assert.Equal($"Entity of type {nameof(ExerciseRecord)} must have a positive ID to be removed.", ex.Message);
    }

    [Fact]
    public async Task RemoveAsync_ExerciseRecordNotExist_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await exerciseRecordRepository.RemoveAsync(2));
        Assert.Equal($"{nameof(ExerciseRecord)} not found.", ex.Message);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldRemoveExerciseRecordsSuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);

        Exercise exercise1 = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise1.Id
        };

        var newExerciseRecord1 = await exerciseRecordRepository.AddAsync(exerciseRecord);

        Exercise exercise2 = await GetPlankExercise(db);
        var exerciseRecord2 = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Time = new TimeSpan(0, 1, 0),
            SumOfTime = new TimeSpan(0, 1, 0),
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise2.Id
        };

        var newExerciseRecord2 = await exerciseRecordRepository.AddAsync(exerciseRecord2);

        //Act
        await exerciseRecordRepository.RemoveRangeAsync(new[] { newExerciseRecord1, newExerciseRecord2 });

        //Assert
        var exerciseRecordById1 = await exerciseRecordRepository.GetByIdAsync(newExerciseRecord1.Id);
        Assert.Null(exerciseRecordById1);

        var exerciseRecordById2 = await exerciseRecordRepository.GetByIdAsync(newExerciseRecord1.Id);
        Assert.Null(exerciseRecordById2);

        var exerciseRecords = await exerciseRecordRepository.GetAllAsync();
        Assert.Empty(exerciseRecords);
        Assert.Equal(0, exerciseRecords.Count());
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);

        Exercise exercise1 = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise1.Id
        };

        Exercise exercise2 = await GetPlankExercise(db);
        var exerciseRecord2 = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Time = new TimeSpan(0, 1, 0),
            SumOfTime = new TimeSpan(0, 1, 0),
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise2.Id
        };

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () =>
            await exerciseRecordRepository.RemoveRangeAsync(new[] { exerciseRecord, exerciseRecord2 }));
        Assert.Equal($"Entities of type {nameof(ExerciseRecord)} must have a positive ID to be removed.", ex.Message);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnExerciseRecords()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise1 = await GetPullUpExercise(db);
        Exercise exercise2 = await GetPlankExercise(db);

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

        //Act
        var addedExerciseRecords = await exerciseRecordRepository.GetAllAsync();

        //Assert
        Assert.NotNull(addedExerciseRecords);
        Assert.Equal(2, addedExerciseRecords.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act
        var addedExerciseRecords = await exerciseRecordRepository.GetAllAsync();

        //Assert
        Assert.NotNull(addedExerciseRecords);
        Assert.Empty(addedExerciseRecords);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnExerciseRecordById()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        var newExerciseRecord = await exerciseRecordRepository.AddAsync(exerciseRecord);

        //Act
        var exerciseRecordById = await exerciseRecordRepository.GetByIdAsync(newExerciseRecord.Id);

        //Assert
        Assert.NotNull(exerciseRecordById);
        Assert.Equal(newExerciseRecord.Id, exerciseRecordById.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act
        var exerciseRecordById = await exerciseRecordRepository.GetByIdAsync(1);

        //Assert
        Assert.Null(exerciseRecordById);
    }

    [Fact]
    public async Task FindAsync_ShouldFindExerciseRecords()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise1 = await GetPullUpExercise(db);
        Exercise exercise2 = await GetPlankExercise(db);

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

        //Act
        var someExerciseRecords = await exerciseRecordRepository.FindAsync(bw => bw.ExerciseId == exercise1.Id);

        //Assert
        Assert.NotNull(someExerciseRecords);
        Assert.Equal(1, someExerciseRecords.Count());

        var result = someExerciseRecords.All(bw => bw.ExerciseId == exercise1.Id);
        Assert.True(result);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise1 = await GetPullUpExercise(db);
        Exercise exercise2 = await GetPlankExercise(db);

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

        //Act
        var someExerciseRecords = await exerciseRecordRepository.FindAsync(bw => bw.ExerciseId != exercise1.Id && bw.ExerciseId != exercise2.Id);

        //Assert
        Assert.Empty(someExerciseRecords);
    }


    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };


        var newExerciseRecord = await exerciseRecordRepository.AddAsync(exerciseRecord);

        //Act
        var exists = await exerciseRecordRepository.ExistsAsync(newExerciseRecord.Id);

        //Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        //Act
        var exists = await exerciseRecordRepository.ExistsAsync(1);

        //Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfull()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        exerciseRecord = await exerciseRecordRepository.AddAsync(exerciseRecord);

        exerciseRecord.CountOfTimes++;;
        exerciseRecord.SumOfReps += 10;

        //Act
        await exerciseRecordRepository.UpdateAsync(exerciseRecord);

        //Assert
        var updatedExerciseRecord = await exerciseRecordRepository.GetByIdAsync(exerciseRecord.Id);

        Assert.NotNull(updatedExerciseRecord);
        Assert.Equal(updatedExerciseRecord, exerciseRecord);
        Assert.Equal(updatedExerciseRecord.CountOfTimes, exerciseRecord.CountOfTimes);
        Assert.Equal(updatedExerciseRecord.SumOfReps, exerciseRecord.SumOfReps);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        exerciseRecord = await exerciseRecordRepository.AddAsync(exerciseRecord);

        exerciseRecord.Id = -1;

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await exerciseRecordRepository.UpdateAsync(exerciseRecord));
        Assert.Equal($"Modified entities of type {nameof(ExerciseRecord)} must have a positive ID.", ex.Message);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesEntitySuccessfully()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        exerciseRecord = await exerciseRecordRepository.AddAsync(exerciseRecord);

        var exerciseRecordById = (await exerciseRecordRepository.GetByIdAsync(exerciseRecord.Id))!;

        exerciseRecord.CountOfTimes++; ;
        exerciseRecord.SumOfReps += 10;

        //Act
        await exerciseRecordRepository.SaveChangesAsync();

        //Assert

        var updatedExerciseRecord = (await exerciseRecordRepository.GetByIdAsync(exerciseRecord.Id))!;
        Assert.NotNull(updatedExerciseRecord);
        Assert.Equal(updatedExerciseRecord, exerciseRecordById);
        Assert.Equal(updatedExerciseRecord.CountOfTimes, exerciseRecordById.CountOfTimes);
        Assert.Equal(updatedExerciseRecord.SumOfReps, exerciseRecordById.SumOfReps);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordRepository = new ExerciseRecordRepository(db);

        User user = await GetDefaultUser(db);
        Exercise exercise = await GetPullUpExercise(db);
        var exerciseRecord = new ExerciseRecord()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Reps = 20,
            SumOfReps = 20,
            CountOfTimes = 1,
            UserId = user.Id,
            ExerciseId = exercise.Id
        };

        exerciseRecord = await exerciseRecordRepository.AddAsync(exerciseRecord);

        var exerciseRecordById = (await exerciseRecordRepository.GetByIdAsync(exerciseRecord.Id))!;

        exerciseRecordById.Id = -1;

        //Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(exerciseRecordRepository.SaveChangesAsync);
    }
}
