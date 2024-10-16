using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;
using WorkoutTrackerAPI.Services;
using Xunit;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Services.BodyWeightServices;
using System.Linq.Expressions;

namespace WorkoutTrackerAPI.Tests.Services.UserServices;

public class ExerciseRecordService_Tests : DbModelService_Tests<ExerciseRecord>
{
    static async Task<Exercise> GetPullUpExerciseAsync(WorkoutDbContext db)
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

    static async Task<Exercise> GetPlankExerciseAsync(WorkoutDbContext db)
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
        User user = await GetDefaultUserAsync(db);
        Exercise exercise = await GetPullUpExerciseAsync(db);

        var validExerciseRecord = new ExerciseRecord()
        {
            Date = DateTime.Now,
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
        Exercise exercise1 = await GetPullUpExerciseAsync(db);
        Exercise exercise2 = await GetPlankExerciseAsync(db);

        var validExerciseRecords = new[]
             {
                new ExerciseRecord()
                {
                    Date = DateTime.Now.AddDays(-15),
                    Reps = 10,
                    SumOfReps = 10,
                    CountOfTimes = 1,
                    ExerciseId = exercise1.Id
                },
            new ExerciseRecord()
                {
                    Date = DateTime.Now.AddDays(-25),
                    Reps = 19,
                    SumOfReps = 29,
                    CountOfTimes = 2,
                    ExerciseId = exercise1.Id
                },
                new ExerciseRecord()
                {
                    Date = DateTime.Now,
                    Reps = 20,
                    SumOfReps = 49,
                    CountOfTimes = 3,
                    ExerciseId = exercise1.Id
                },
                new ExerciseRecord()
                {
                    Date = DateTime.Now,
                    Time = new TimeSpan(0, 1, 0),
                    SumOfTime= new TimeSpan(0, 1, 0),
                    CountOfTimes = 1,
                    ExerciseId = exercise2.Id
                }
            };

        return validExerciseRecords;
    }

    static IExerciseRecordService GetExerciseRecordService(WorkoutDbContext db)
    {
        var exerciseRecordRepository = new ExerciseRecordRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new ExerciseRecordService(exerciseRecordRepository, userRepository);
    }


    [Fact]
    public async Task AddExerciseRecordToUser_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, validExerciseRecord);

        // Assert
        Assert.True(result.Success);
        Assert.NotEqual(0, validExerciseRecord.Id);
        Assert.Equal(validExerciseRecord, result.Model);
    }

    [Fact]
    public async Task AddExerciseRecordToUser_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);

        // Act
        var result = await exerciseRecordService.AddExerciseRecordToUserAsync(null!, validExerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddExerciseRecordToUser_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await exerciseRecordService.AddExerciseRecordToUserAsync(nonExistenceUserId, validExerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddExerciseRecordToUser_ShouldReturnFail_WhenExerciseRecordIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Exercise record entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddExerciseRecordToUser_ShouldReturnFail_WhenExerciseRecordIdIsNonZero()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);
        var validExerciseRecord = await GetValidExerciseRecordAsync(db);
        validExerciseRecord.Id = 1;

        // Act
        var result = await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, validExerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("ExerciseRecord ID must not be set when adding a new exercise record.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddExerciseRecordToUser_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordRepositoryMock = new Mock<ExerciseRecordRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseRecordService = new ExerciseRecordService(exerciseRecordRepositoryMock.Object, userRepository);

        var validExerciseRecord = await GetValidExerciseRecordAsync(db);

        var user = await GetDefaultUserAsync(db);

        exerciseRecordRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<ExerciseRecord>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, validExerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to add exercise record", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseRecordFromUser_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        var user = await GetDefaultUserAsync(db);
        string userId = user.Id;

        await exerciseRecordService.AddExerciseRecordToUserAsync(userId, exerciseRecord);

        // Act
        var result = await exerciseRecordService.DeleteExerciseRecordFromUserAsync(userId, exerciseRecord.Id);

        // Assert
        Assert.True(result.Success);

        var resultById = await exerciseRecordService.GetUserExerciseRecordByIdAsync(userId, exerciseRecord.Id);
        Assert.Null(resultById.Model);
    }

    [Fact]
    public async Task DeleteExerciseRecordFromUser_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        // Act
        var result = await exerciseRecordService.DeleteExerciseRecordFromUserAsync(null!, exerciseRecord.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseRecordFromUser_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await exerciseRecordService.DeleteExerciseRecordFromUserAsync(nonExistenceUserId, exerciseRecord.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseRecordFromUser_ShouldReturnFail_WhenInvalidExerciseRecordID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);
        long invalidExerciseRecordID = -1;

        // Act
        var result = await exerciseRecordService.DeleteExerciseRecordFromUserAsync(user.Id, invalidExerciseRecordID);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid ExerciseRecord ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseRecordFromUser_ShouldReturnFail_WhenExerciseRecordNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        long nonExistenceExerciseRecordId = 100;

        // Act
        var result = await exerciseRecordService.DeleteExerciseRecordFromUserAsync(user.Id, nonExistenceExerciseRecordId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Exercise record not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseRecordFromUser_ShouldReturnFail_AccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordRepositoryMock = new Mock<ExerciseRecordRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseRecordService = new ExerciseRecordService(exerciseRecordRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        exerciseRecord.Id = 1;

        exerciseRecordRepositoryMock
            .Setup(repo => repo.GetByIdAsync(exerciseRecord.Id))
            .ReturnsAsync(() =>
            {
                exerciseRecord.UserId = Guid.NewGuid().ToString();
                return exerciseRecord;
            });

        // Act
        var result = await exerciseRecordService.DeleteExerciseRecordFromUserAsync(user.Id, exerciseRecord.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User does not have permission to delete this exercise record entry", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseRecordFromUser_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordRepositoryMock = new Mock<ExerciseRecordRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseRecordService = new ExerciseRecordService(exerciseRecordRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        exerciseRecord.Id = 1;
        exerciseRecord.UserId = user.Id;

        exerciseRecordRepositoryMock
            .Setup(repo => repo.GetByIdAsync(exerciseRecord.Id))
            .ReturnsAsync(() => exerciseRecord);

        exerciseRecordRepositoryMock
            .Setup(repo => repo.RemoveAsync(exerciseRecord.Id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseRecordService.DeleteExerciseRecordFromUserAsync(user.Id, exerciseRecord.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to delete exercise record", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseRecords_ShouldReturnExerciseRecords_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var exerciseRecords = await GetValidExerciseRecordsAsync(db);
        var user = await GetDefaultUserAsync(db);

        foreach (var exerciseRecord in exerciseRecords)
        {
            await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);
        }

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordsAsync(user.Id, pullUpExercise.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.All(result.Model, m => Assert.True(m.ExerciseId == pullUpExercise.Id));
    }

    [Fact]
    public async Task GetUserExerciseRecords_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordsAsync(null!, pullUpExercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseRecords_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordsAsync(nonExistenceUserId, pullUpExercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseRecords_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordRepositoryMock = new Mock<ExerciseRecordRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseRecordService = new ExerciseRecordService(exerciseRecordRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        exerciseRecordRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<ExerciseRecord, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordsAsync(user.Id, pullUpExercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get exercise records", result.ErrorMessage);
    }


    [Fact]
    public async Task GetUserExerciseRecordByDate_ShouldReturnExerciseRecordByDate_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        var exerciseRecords = await GetValidExerciseRecordsAsync(db);
        foreach (var exerciseRecord in exerciseRecords)
        {
            await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByDateAsync(user.Id, pullUpExercise.Id, today);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(today, DateOnly.FromDateTime(result.Model.Date));
    }

    [Fact]
    public async Task GetUserExerciseRecordByDate_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        var exerciseRecords = await GetValidExerciseRecordsAsync(db);
        foreach (var exerciseRecord in exerciseRecords)
        {
            await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);
        }

        DateOnly yesterday = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByDateAsync(user.Id, pullUpExercise.Id, yesterday);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserExerciseRecordByDate_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByDateAsync(null!, pullUpExercise.Id, today);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseRecordByDate_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByDateAsync(nonExistenceUserId, pullUpExercise.Id, today);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseRecordByDate_ShouldReturnFail_WhenInvalidDate()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        var exerciseRecords = await GetValidExerciseRecordsAsync(db);
        foreach (var exerciseRecord in exerciseRecords)
        {
            await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);
        }

        DateOnly tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByDateAsync(user.Id, pullUpExercise.Id, tomorrow);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Incorrect date.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseRecordByDate_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordRepositoryMock = new Mock<ExerciseRecordRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseRecordService = new ExerciseRecordService(exerciseRecordRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        exerciseRecordRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<ExerciseRecord, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByDateAsync(user.Id, pullUpExercise.Id, today);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get exercise record by date", result.ErrorMessage);
    }


    [Fact]
    public async Task GetUserExerciseRecordById_ShouldReturnExerciseRecordById_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByIdAsync(user.Id, exerciseRecord.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(exerciseRecord.Id, result.Model.Id);
    }

    [Fact]
    public async Task GetUserExerciseRecordById_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByIdAsync(user.Id, 1);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserExerciseRecordById_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByIdAsync(null!, exerciseRecord.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseRecordById_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByIdAsync(nonExistenceUserId, exerciseRecord.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseRecordById_ShouldReturnFail_WhenInvalidExerciseRecordID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByIdAsync(user.Id, -1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid ExerciseRecord ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseRecordById_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordRepositoryMock = new Mock<ExerciseRecordRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseRecordService = new ExerciseRecordService(exerciseRecordRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        exerciseRecord.Id = 1;

        exerciseRecordRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await exerciseRecordService.GetUserExerciseRecordByIdAsync(user.Id, exerciseRecord.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get exercise record", result.ErrorMessage);
    }



    [Fact]
    public async Task UpdateUserExerciseRecord_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        exerciseRecord.Reps = 45;

        // Act
        var result = await exerciseRecordService.UpdateUserExerciseRecordAsync(user.Id, exerciseRecord);

        // Assert
        Assert.True(result.Success);

        var resultById = await exerciseRecordService.GetUserExerciseRecordByIdAsync(user.Id, exerciseRecord.Id);
        Assert.NotNull(resultById.Model);
        Assert.Equal(exerciseRecord.Reps, resultById.Model!.Reps);
    }

    [Fact]
    public async Task UpdateUserExerciseRecord_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        exerciseRecord.Reps = 45;

        // Act
        var result = await exerciseRecordService.UpdateUserExerciseRecordAsync(null!, exerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExerciseRecord_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        exerciseRecord.Reps = 45;

        // Act
        var result = await exerciseRecordService.UpdateUserExerciseRecordAsync(nonExistenceUserId, exerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExerciseRecord_ShouldReturnFail_WhenExerciseRecordIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseRecordService.UpdateUserExerciseRecordAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Exercise record entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExerciseRecord_ShouldReturnFail_WhenInvalidExerciseRecordID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        exerciseRecord.Id = -1;
        exerciseRecord.Reps = 45;

        // Act
        var result = await exerciseRecordService.UpdateUserExerciseRecordAsync(user.Id, exerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid ExerciseRecord ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExerciseRecord_ShouldReturnFail_WhenExerciseRecordNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);

        var user = await GetDefaultUserAsync(db);
        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        exerciseRecord.Id = 1;
        exerciseRecord.Reps = 45;

        // Act
        var result = await exerciseRecordService.UpdateUserExerciseRecordAsync(user.Id, exerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Exercise record not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExerciseRecord_ShouldReturnFail_WhenAccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordRepositoryMock = new Mock<ExerciseRecordRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseRecordService = new ExerciseRecordService(exerciseRecordRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        exerciseRecord.Id = 1;
        exerciseRecord.Reps = 45;
        exerciseRecord.UserId = Guid.NewGuid().ToString();

        exerciseRecordRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => exerciseRecord);

        // Act
        var result = await exerciseRecordService.UpdateUserExerciseRecordAsync(user.Id, exerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User does not have permission to update this exercise record entry", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExerciseRecord_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRecordRepositoryMock = new Mock<ExerciseRecordRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseRecordService = new ExerciseRecordService(exerciseRecordRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        exerciseRecord.Id = 1;
        exerciseRecord.UserId = user.Id;

        exerciseRecordRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => exerciseRecord);

        exerciseRecordRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<ExerciseRecord>()))
            .ThrowsAsync(new Exception("Database error"));

        exerciseRecord.Reps = 45;

        // Act
        var result = await exerciseRecordService.UpdateUserExerciseRecordAsync(user.Id, exerciseRecord);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to update exercise record", result.ErrorMessage);
    }
}
