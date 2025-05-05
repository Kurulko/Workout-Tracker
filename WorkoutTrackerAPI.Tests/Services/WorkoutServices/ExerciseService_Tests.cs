using Moq;
using System.Linq.Expressions;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Exceptions;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Repositories.UserRepositories;
using WorkoutTracker.API.Services;
using WorkoutTracker.API.Services.ExerciseServices;
using Xunit;

namespace WorkoutTracker.API.Tests.Services.WorkoutServices;

public class ExerciseService_Tests : BaseWorkoutService_Tests<Exercise>
{
    static async Task<Exercise> CreateExerciseAsync(WorkoutDbContext db, string name, ExerciseType exerciseType, params string[] muscleNames)
    {
        var muscleRepository = new MuscleRepository(db);

        Exercise exercise = new();

        exercise.Name = name;
        exercise.Type = exerciseType;

        var muscles = new List<Muscle>();

        foreach (string muscleName in muscleNames)
        {
            var muscle = await muscleRepository.GetByNameAsync(muscleName);
            if (muscle is not null)
                muscles.Add(muscle);
        }

        exercise.WorkingMuscles = muscles;
        return exercise;

    }

    static async Task<Exercise> GetValidExerciseAsync(WorkoutDbContext db)
    {
        return await CreateExerciseAsync(db, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
    }

    static async Task<IEnumerable<Exercise>> GetValidExercisesAsync(WorkoutDbContext db)
    {
        var plankExercise = await CreateExerciseAsync(db, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
        var pullUpExercise = await CreateExerciseAsync(db, "Pull Up", ExerciseType.Reps, "Latissimus dorsi", "Biceps brachii", "Teres minor");
        var pushUpExercise = await CreateExerciseAsync(db, "Push Up", ExerciseType.Reps, "Pectoralis major", "Triceps brachii", "Deltoids");

        var validExercises = new[] { plankExercise, pullUpExercise, pushUpExercise };

        return validExercises;
    }

    static IExerciseService GetExerciseService(WorkoutDbContext db)
    {
        var exerciseRepository = new ExerciseRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new ExerciseService(exerciseRepository, userRepository);
    }


    [Fact]
    public async Task AddUserExercise_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var validExercise = await GetValidExerciseAsync(db);
        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseService.AddUserExerciseAsync(user.Id, validExercise);

        // Assert
        Assert.True(result.Success);
        Assert.NotEqual(0, validExercise.Id);
        Assert.Equal(validExercise, result.Model);
    }

    [Fact]
    public async Task AddUserExercise_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var validExercise = await GetValidExerciseAsync(db);

        // Act
        var result = await exerciseService.AddUserExerciseAsync(null!, validExercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddUserExercise_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var validExercise = await GetValidExerciseAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await exerciseService.AddUserExerciseAsync(nonExistenceUserId, validExercise);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddUserExercise_ShouldReturnFail_WhenExerciseIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseService.AddUserExerciseAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Exercise entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddUserExercise_ShouldReturnFail_WhenExerciseIdIsNonZero()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);
        var validExercise = await GetValidExerciseAsync(db);
        validExercise.Id = 1;

        // Act
        var result = await exerciseService.AddUserExerciseAsync(user.Id, validExercise);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Exercise ID must not be set when adding a new exercise.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddUserExercise_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var validExercise = await GetValidExerciseAsync(db);

        var user = await GetDefaultUserAsync(db);

        exerciseRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Exercise>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseService.AddUserExerciseAsync(user.Id, validExercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to add user exercise", result.ErrorMessage);
    }



    [Fact]
    public async Task AddExercise_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var validExercise = await GetValidExerciseAsync(db);

        // Act
        var result = await exerciseService.AddExerciseAsync(validExercise);

        // Assert
        Assert.True(result.Success);
        Assert.NotEqual(0, validExercise.Id);
        Assert.Equal(validExercise, result.Model);
    }

    [Fact]
    public async Task AddExercise_ShouldReturnFail_WhenExerciseCreatedByUser()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);
        var validExercise = await GetValidExerciseAsync(db);

        validExercise.CreatedByUserId = user.Id;

        // Act
        var result = await exerciseService.AddExerciseAsync(validExercise);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Exercise entry cannot be created by user.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddExercise_ShouldReturnFail_WhenExerciseIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        // Act
        var result = await exerciseService.AddExerciseAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Exercise entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddExercise_ShouldReturnFail_WhenExerciseIdIsNonZero()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var validExercise = await GetValidExerciseAsync(db);
        validExercise.Id = 1;

        // Act
        var result = await exerciseService.AddExerciseAsync(validExercise);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Exercise ID must not be set when adding a new exercise.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddExercise_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var validExercise = await GetValidExerciseAsync(db);

        exerciseRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Exercise>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseService.AddExerciseAsync(validExercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to add exercise", result.ErrorMessage);
    }



    [Fact]
    public async Task DeleteExerciseFromUser_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);
        var user = await GetDefaultUserAsync(db);
        string userId = user.Id;

        await exerciseService.AddUserExerciseAsync(userId, exercise);

        // Act
        var result = await exerciseService.DeleteExerciseFromUserAsync(userId, exercise.Id);

        // Assert
        Assert.True(result.Success);

        var resultById = await exerciseService.GetUserExerciseByIdAsync(userId, exercise.Id);
        Assert.Null(resultById.Model);
    }

    [Fact]
    public async Task DeleteExerciseFromUser_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);

        // Act
        var result = await exerciseService.DeleteExerciseFromUserAsync(null!, exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseFromUser_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await exerciseService.DeleteExerciseFromUserAsync(nonExistenceUserId, exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseFromUser_ShouldReturnFail_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);
        long invalidExerciseID = -1;

        // Act
        var result = await exerciseService.DeleteExerciseFromUserAsync(user.Id, invalidExerciseID);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Exercise ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseFromUser_ShouldReturnFail_WhenExerciseNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        long nonExistenceExerciseId = 100;

        // Act
        var result = await exerciseService.DeleteExerciseFromUserAsync(user.Id, nonExistenceExerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Exercise not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseFromUser_ShouldReturnFail_AccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(exercise.Id))
            .ReturnsAsync(() =>
            {
                exercise.CreatedByUserId = Guid.NewGuid().ToString();
                return exercise;
            });

        // Act
        var result = await exerciseService.DeleteExerciseFromUserAsync(user.Id, exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User does not have permission to delete this exercise entry", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExerciseFromUser_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;
        exercise.CreatedByUserId = user.Id;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(exercise.Id))
            .ReturnsAsync(() => exercise);

        exerciseRepositoryMock
            .Setup(repo => repo.RemoveAsync(exercise.Id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseService.DeleteExerciseFromUserAsync(user.Id, exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to delete user exercise", result.ErrorMessage);
    }


    [Fact]
    public async Task DeleteExercise_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);

        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exerciseService.DeleteExerciseAsync(exercise.Id);

        // Assert
        Assert.True(result.Success);

        var resultById = await exerciseService.GetExerciseByIdAsync(exercise.Id);
        Assert.Null(resultById.Model);
    }

    [Fact]
    public async Task DeleteExercise_ShouldReturnFail_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        long invalidExerciseID = -1;

        // Act
        var result = await exerciseService.DeleteExerciseAsync(invalidExerciseID);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Exercise ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExercise_ShouldReturnFail_WhenExerciseNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        long nonExistenceExerciseId = 100;

        // Act
        var result = await exerciseService.DeleteExerciseAsync(nonExistenceExerciseId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Exercise not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExercise_ShouldReturnFail_AccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(exercise.Id))
            .ReturnsAsync(() =>
            {
                exercise.CreatedByUserId = Guid.NewGuid().ToString();
                return exercise;
            });

        // Act
        var result = await exerciseService.DeleteExerciseAsync(exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User does not have permission to delete this exercise entry", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteExercise_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(exercise.Id))
            .ReturnsAsync(() => exercise);

        exerciseRepositoryMock
            .Setup(repo => repo.RemoveAsync(exercise.Id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseService.DeleteExerciseAsync(exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to delete exercise", result.ErrorMessage);
    }



    [Fact]
    public async Task GetUserExercises_ShouldReturnExercises_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercises = await GetValidExercisesAsync(db);
        var user = await GetDefaultUserAsync(db);

        foreach (var exercise in exercises)
        {
            await exerciseService.AddUserExerciseAsync(user.Id, exercise);
        }

        // Act
        var result = await exerciseService.GetUserExercisesAsync(user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.All(result.Model, m => Assert.True(m.CreatedByUserId == user.Id));
    }

    [Fact]
    public async Task GetUserExercises_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        // Act
        var result = await exerciseService.GetUserExercisesAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExercises_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await exerciseService.GetUserExercisesAsync(nonExistenceUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExercises_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);

        exerciseRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Exercise, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));


        // Act
        var result = await exerciseService.GetUserExercisesAsync(user.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get user exercises", result.ErrorMessage);
    }



    [Fact]
    public async Task GetExercises_ShouldReturnExercises_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercises = await GetValidExercisesAsync(db);

        foreach (var exercise in exercises)
        {
            await exerciseService.AddExerciseAsync(exercise);
        }

        // Act
        var result = await exerciseService.GetExercisesAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(exercises.Count(), result.Model.Count());
    }

    [Fact]
    public async Task GetExercises_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var exercise = await GetValidExerciseAsync(db);

        exerciseRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseService.GetExercisesAsync();

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get exercises", result.ErrorMessage);
    }



    [Fact]
    public async Task GetUserExerciseByName_ShouldReturnExerciseByName_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddUserExerciseAsync(user.Id, exercise);
        }

        string exerciseName = "Pull Up";

        // Act
        var result = await exerciseService.GetUserExerciseByNameAsync(user.Id, exerciseName);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(exerciseName, result.Model.Name);
    }

    [Fact]
    public async Task GetUserExerciseByName_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddUserExerciseAsync(user.Id, exercise);
        }

        string nonExistenceExercise = "Non-existence exercise";

        // Act
        var result = await exerciseService.GetUserExerciseByNameAsync(user.Id, nonExistenceExercise);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserExerciseByName_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        string exerciseName = "Pull Up";

        // Act
        var result = await exerciseService.GetUserExerciseByNameAsync(null!, exerciseName);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseByName_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();
        string exerciseName = "Non-existence exercise";

        // Act
        var result = await exerciseService.GetUserExerciseByNameAsync(nonExistenceUserId, exerciseName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseByName_ShouldReturnFail_WhenInvalidName()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddUserExerciseAsync(user.Id, exercise);
        }

        // Act
        var result = await exerciseService.GetUserExerciseByNameAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Exercise name cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseByName_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;
        exercise.CreatedByUserId = user.Id;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseService.GetUserExerciseByNameAsync(user.Id, exercise.Name);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get user exercise by name", result.ErrorMessage);
    }


    [Fact]
    public async Task GetExerciseByName_ShouldReturnExerciseByName_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);


        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddExerciseAsync(exercise);
        }

        string exerciseName = "Pull Up";

        // Act
        var result = await exerciseService.GetExerciseByNameAsync(exerciseName);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(exerciseName, result.Model.Name);
    }

    [Fact]
    public async Task GetExerciseByName_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddExerciseAsync(exercise);
        }

        string nonExistenceExercise = "Non-existence exercise";

        // Act
        var result = await exerciseService.GetExerciseByNameAsync(nonExistenceExercise);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetExerciseByName_ShouldReturnFail_WhenInvalidName()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddExerciseAsync(exercise);
        }

        // Act
        var result = await exerciseService.GetExerciseByNameAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Exercise name cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetExerciseByName_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseService.GetExerciseByNameAsync(exercise.Name);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get exercise by name", result.ErrorMessage);
    }


    [Fact]
    public async Task GetUserExerciseById_ShouldReturnExerciseById_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exerciseService.GetUserExerciseByIdAsync(user.Id, exercise.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(exercise.Id, result.Model.Id);
    }

    [Fact]
    public async Task GetUserExerciseById_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseService.GetUserExerciseByIdAsync(user.Id, 1);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserExerciseById_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exerciseService.GetUserExerciseByIdAsync(null!, exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseById_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exerciseService.GetUserExerciseByIdAsync(nonExistenceUserId, exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseById_ShouldReturnFail_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseService.GetUserExerciseByIdAsync(user.Id, -1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Exercise ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserExerciseById_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseService.GetUserExerciseByIdAsync(user.Id, exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get user exercise", result.ErrorMessage);
    }


    [Fact]
    public async Task GetExerciseById_ShouldReturnExerciseById_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exerciseService.GetExerciseByIdAsync(exercise.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(exercise.Id, result.Model.Id);
    }

    [Fact]
    public async Task GetExerciseById_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        // Act
        var result = await exerciseService.GetExerciseByIdAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetExerciseById_ShouldReturnFail_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        // Act
        var result = await exerciseService.GetExerciseByIdAsync(-1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Exercise ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetExerciseById_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await exerciseService.GetExerciseByIdAsync( exercise.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get exercise", result.ErrorMessage);
    }



    [Fact]
    public async Task UpdateUserExercise_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateUserExerciseAsync(user.Id, exercise);

        // Assert
        Assert.True(result.Success);

        var resultById = await exerciseService.GetUserExerciseByIdAsync(user.Id, exercise.Id);
        Assert.NotNull(resultById.Model);
        Assert.Equal(exercise.Name, resultById.Model!.Name);
    }

    [Fact]
    public async Task UpdateUserExercise_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateUserExerciseAsync(null!, exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExercise_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateUserExerciseAsync(nonExistenceUserId, exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExercise_ShouldReturnFail_WhenExerciseIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseService.UpdateUserExerciseAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Exercise entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExercise_ShouldReturnFail_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        exercise.Id = -1;
        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateUserExerciseAsync(user.Id, exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Exercise ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExercise_ShouldReturnFail_WhenExerciseNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;
        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateUserExerciseAsync(user.Id, exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Exercise not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExercise_ShouldReturnFail_WhenAccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;
        exercise.Name = "New Name";
        exercise.CreatedByUserId = Guid.NewGuid().ToString();

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => exercise);

        // Act
        var result = await exerciseService.UpdateUserExerciseAsync(user.Id, exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User does not have permission to update this exercise entry", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserExercise_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;
        exercise.CreatedByUserId = user.Id;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => exercise);

        exerciseRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Exercise>()))
            .ThrowsAsync(new Exception("Database error"));

        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateUserExerciseAsync(user.Id, exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to update user exercise", result.ErrorMessage);
    }


    [Fact]
    public async Task UpdateExercise_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);


        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateExerciseAsync(exercise);

        // Assert
        Assert.True(result.Success);

        var resultById = await exerciseService.GetExerciseByIdAsync(exercise.Id);
        Assert.NotNull(resultById.Model);
        Assert.Equal(exercise.Name, resultById.Model!.Name);
    }


    [Fact]
    public async Task UpdateExercise_ShouldReturnFail_WhenExerciseIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        // Act
        var result = await exerciseService.UpdateExerciseAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Exercise entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateExercise_ShouldReturnFail_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        exercise.Id = -1;
        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateExerciseAsync(exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Exercise ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateExercise_ShouldReturnFail_WhenExerciseNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;
        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateExerciseAsync(exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Exercise not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateExercise_ShouldReturnFail_WhenAccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;
        exercise.Name = "New Name";
        exercise.CreatedByUserId = Guid.NewGuid().ToString();

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => exercise);

        // Act
        var result = await exerciseService.UpdateExerciseAsync(exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User does not have permission to update this exercise entry", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateExercise_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => exercise);

        exerciseRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Exercise>()))
            .ThrowsAsync(new Exception("Database error"));

        exercise.Name = "New Name";

        // Act
        var result = await exerciseService.UpdateExerciseAsync(exercise);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to update exercise", result.ErrorMessage);
    }


    [Fact]
    public async Task UserExerciseExists_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exerciseService.UserExerciseExistsAsync(user.Id, exercise.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserExerciseExists_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await exerciseService.UserExerciseExistsAsync(user.Id, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserExerciseExists_ShouldThrowException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await exerciseService.UserExerciseExistsAsync(null!, exercise.Id));
        Assert.Contains("User ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task UserExerciseExists_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await exerciseService.UserExerciseExistsAsync(nonExistenceUserId, exercise.Id));
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task UserExerciseExists_ShouldThrowException_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        exercise.Id = -1;

        //Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidIDException>(async () => await exerciseService.UserExerciseExistsAsync(user.Id, exercise.Id));
        Assert.Contains("Invalid Exercise ID.", ex.Message);
    }

    [Fact]
    public async Task UserExerciseExists_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.ExistsAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await exerciseService.UserExerciseExistsAsync(user.Id, exercise.Id));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task ExerciseExists_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exerciseService.ExerciseExistsAsync(exercise.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExerciseExists_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        // Act
        var result = await exerciseService.ExerciseExistsAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExerciseExists_ShouldThrowException_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        exercise.Id = -1;

        //Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidIDException>(async () => await exerciseService.ExerciseExistsAsync(exercise.Id));
        Assert.Contains("Invalid Exercise ID.", ex.Message);
    }

    [Fact]
    public async Task ExerciseExists_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.ExistsAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await exerciseService.ExerciseExistsAsync(exercise.Id));
        Assert.Equal("Database error", ex.Message);
    }



    [Fact]
    public async Task UserExerciseExistsByName_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exerciseService.UserExerciseExistsByNameAsync(user.Id, exercise.Name);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserExerciseExistsByName_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);
        string nonExistenceName = "Non-existence name";

        // Act
        var result = await exerciseService.UserExerciseExistsByNameAsync(user.Id, nonExistenceName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserExerciseExistsByName_ShouldThrowException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await exerciseService.UserExerciseExistsByNameAsync(null!, exercise.Name));
        Assert.Contains("User ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task UserExerciseExistsByName_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await exerciseService.UserExerciseExistsByNameAsync(nonExistenceUserId, exercise.Name));
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task UserExerciseExistsByName_ShouldThrowException_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var user = await GetDefaultUserAsync(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await exerciseService.UserExerciseExistsByNameAsync(user.Id, null!));
        Assert.Contains("Exercise name cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task UserExerciseExistsByName_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var exercise = await GetValidExerciseAsync(db);

        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.ExistsByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await exerciseService.UserExerciseExistsByNameAsync(user.Id, exercise.Name));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task ExerciseExistsByName_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exerciseService.ExerciseExistsByNameAsync(exercise.Name);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task ExerciseExistsByName_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        string nonExistenceName = "Non-existence name";

        // Act
        var result = await exerciseService.ExerciseExistsByNameAsync(nonExistenceName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ExerciseExistsByName_ShouldThrowException_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await exerciseService.ExerciseExistsByNameAsync(null!));
        Assert.Contains("Exercise name cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task ExerciseExistsByName_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepositoryMock = new Mock<ExerciseRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var exerciseService = new ExerciseService(exerciseRepositoryMock.Object, userRepository);

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1;

        exerciseRepositoryMock
            .Setup(repo => repo.ExistsByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await exerciseService.ExerciseExistsByNameAsync(exercise.Name));
        Assert.Equal("Database error", ex.Message);
    }

}
