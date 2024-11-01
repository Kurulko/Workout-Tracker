using Moq;
using System.Linq.Expressions;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.WorkoutServices;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Services.WorkoutServices;

public class WorkoutService_Tests : BaseWorkoutService_Tests<Workout>
{
    static async Task<Exercise> GetPlankExercise(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);
        var exerciseRepository = new ExerciseRepository(db);

        return await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
    }

    static async Task<Exercise> GetLegRaiseExercise(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);
        var exerciseRepository = new ExerciseRepository(db);

        return await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Leg Raise", ExerciseType.Reps, "Rectus abdominis", "Hip flexors");
    }

    static async Task<Exercise> GetStandingCalfRaisesExercise(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);
        var exerciseRepository = new ExerciseRepository(db);

        return await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Standing Calf Raises", ExerciseType.WeightAndReps, "Gastrocnemius", "Soleus");
    }

    async Task<Workout> GetValidWorkoutAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);

        var plankExercise = await GetPlankExercise(db);
        var legRaiseExercise = await GetLegRaiseExercise(db);

        var validWorkout = new Workout()
        {
            Name = "Abs and legs",
            Exercises = new[] { plankExercise, legRaiseExercise },
            UserId = user.Id
        };

        return validWorkout;
    }

    async Task<IEnumerable<Workout>> GetValidWorkoutsAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var plankExercise = await GetPlankExercise(db);
        var legRaiseExercise = await GetLegRaiseExercise(db);
        var standingCalfRaisesExercise = await GetStandingCalfRaisesExercise(db);

        var validWorkouts = new[] {
            new Workout()
            {
                Name = "Abs and legs",
                Exercises = new[] { plankExercise, legRaiseExercise },
                UserId = user.Id
            },
            new Workout()
            {
                Name = "Legs",
                Exercises = new[] { standingCalfRaisesExercise, legRaiseExercise },
                UserId = user.Id
            }
        };

        return validWorkouts;
    }

    static IWorkoutService GetWorkoutService(WorkoutDbContext db)
    {
        var workoutRepository = new WorkoutRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new WorkoutService(workoutRepository, userRepository);
    }


    [Fact]
    public async Task AddUserWorkout_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await workoutService.AddUserWorkoutAsync(user.Id, validWorkout);

        // Assert
        Assert.True(result.Success);
        Assert.NotEqual(0, validWorkout.Id);
        Assert.Equal(validWorkout, result.Model);
    }

    [Fact]
    public async Task AddUserWorkout_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var validWorkout = await GetValidWorkoutAsync(db);

        // Act
        var result = await workoutService.AddUserWorkoutAsync(null!, validWorkout);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddUserWorkout_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await workoutService.AddUserWorkoutAsync(nonExistenceUserId, validWorkout);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddUserWorkout_ShouldReturnFail_WhenWorkoutIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await workoutService.AddUserWorkoutAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Workout entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddUserWorkout_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var validWorkout = await GetValidWorkoutAsync(db);

        var user = await GetDefaultUserAsync(db);

        workoutRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Workout>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await workoutService.AddUserWorkoutAsync(user.Id, validWorkout);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to add workout", result.ErrorMessage);
    }


    [Fact]
    public async Task DeleteWorkoutFromUser_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var workout = await GetValidWorkoutAsync(db);
        var user = await GetDefaultUserAsync(db);
        string userId = user.Id;

        await workoutService.AddUserWorkoutAsync(userId, workout);

        // Act
        var result = await workoutService.DeleteUserWorkoutAsync(userId, workout.Id);

        // Assert
        Assert.True(result.Success);

        var resultById = await workoutService.GetUserWorkoutByIdAsync(userId, workout.Id);
        Assert.Null(resultById.Model);
    }

    [Fact]
    public async Task DeleteWorkoutFromUser_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var workout = await GetValidWorkoutAsync(db);

        // Act
        var result = await workoutService.DeleteUserWorkoutAsync(null!, workout.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteWorkoutFromUser_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var workout = await GetValidWorkoutAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await workoutService.DeleteUserWorkoutAsync(nonExistenceUserId, workout.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteWorkoutFromUser_ShouldReturnFail_WhenInvalidWorkoutID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);
        long invalidWorkoutID = -1;

        // Act
        var result = await workoutService.DeleteUserWorkoutAsync(user.Id, invalidWorkoutID);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Workout ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteWorkoutFromUser_ShouldReturnFail_WhenWorkoutNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        long nonExistenceWorkoutId = 100;

        // Act
        var result = await workoutService.DeleteUserWorkoutAsync(user.Id, nonExistenceWorkoutId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Workout not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteWorkoutFromUser_ShouldReturnFail_AccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);

        workout.Id = 1;

        workoutRepositoryMock
            .Setup(repo => repo.GetByIdAsync(workout.Id))
            .ReturnsAsync(() =>
            {
                workout.UserId = Guid.NewGuid().ToString();
                return workout;
            });

        // Act
        var result = await workoutService.DeleteUserWorkoutAsync(user.Id, workout.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User does not have permission to delete this workout entry", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteWorkoutFromUser_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);

        workout.Id = 1;
        workout.UserId = user.Id;

        workoutRepositoryMock
            .Setup(repo => repo.GetByIdAsync(workout.Id))
            .ReturnsAsync(() => workout);

        workoutRepositoryMock
            .Setup(repo => repo.RemoveAsync(workout.Id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await workoutService.DeleteUserWorkoutAsync(user.Id, workout.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to delete workout", result.ErrorMessage);
    }



    [Fact]
    public async Task GetUserWorkouts_ShouldReturnWorkouts_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var workouts = await GetValidWorkoutsAsync(db);
        var user = await GetDefaultUserAsync(db);

        foreach (var workout in workouts)
        {
            await workoutService.AddUserWorkoutAsync(user.Id, workout);
        }

        // Act
        var result = await workoutService.GetUserWorkoutsAsync(user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.All(result.Model, m => Assert.True(m.UserId == user.Id));
    }

    [Fact]
    public async Task GetUserWorkouts_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        // Act
        var result = await workoutService.GetUserWorkoutsAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserWorkouts_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await workoutService.GetUserWorkoutsAsync(nonExistenceUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserWorkouts_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);

        workoutRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<Workout, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));


        // Act
        var result = await workoutService.GetUserWorkoutsAsync(user.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get workouts", result.ErrorMessage);
    }


    [Fact]
    public async Task GetUserWorkoutByName_ShouldReturnWorkoutByName_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workouts = await GetValidWorkoutsAsync(db);
        foreach (var workout in workouts)
        {
            await workoutService.AddUserWorkoutAsync(user.Id, workout);
        }

        string workoutName = "Legs";

        // Act
        var result = await workoutService.GetUserWorkoutByNameAsync(user.Id, workoutName);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(workoutName, result.Model.Name);
    }

    [Fact]
    public async Task GetUserWorkoutByName_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workouts = await GetValidWorkoutsAsync(db);
        foreach (var workout in workouts)
        {
            await workoutService.AddUserWorkoutAsync(user.Id, workout);
        }

        string nonExistenceWorkout = "Non-existence workout";

        // Act
        var result = await workoutService.GetUserWorkoutByNameAsync(user.Id, nonExistenceWorkout);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserWorkoutByName_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        string workoutName = "Legs";

        // Act
        var result = await workoutService.GetUserWorkoutByNameAsync(null!, workoutName);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserWorkoutByName_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();
        string workoutName = "Non-existence workout";

        // Act
        var result = await workoutService.GetUserWorkoutByNameAsync(nonExistenceUserId, workoutName);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserWorkoutByName_ShouldReturnFail_WhenInvalidName()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workouts = await GetValidWorkoutsAsync(db);
        foreach (var workout in workouts)
        {
            await workoutService.AddUserWorkoutAsync(user.Id, workout);
        }

        // Act
        var result = await workoutService.GetUserWorkoutByNameAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Workout name cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserWorkoutByName_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);

        workout.Id = 1;
        workout.UserId = user.Id;

        workoutRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await workoutService.GetUserWorkoutByNameAsync(user.Id, workout.Name);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get workout by name", result.ErrorMessage);
    }


    [Fact]
    public async Task GetUserWorkoutById_ShouldReturnWorkoutById_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutService.GetUserWorkoutByIdAsync(user.Id, workout.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(workout.Id, result.Model.Id);
    }

    [Fact]
    public async Task GetUserWorkoutById_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await workoutService.GetUserWorkoutByIdAsync(user.Id, 1);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserWorkoutById_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutService.GetUserWorkoutByIdAsync(null!, workout.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserWorkoutById_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutService.GetUserWorkoutByIdAsync(nonExistenceUserId, workout.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserWorkoutById_ShouldReturnFail_WhenInvalidWorkoutID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await workoutService.GetUserWorkoutByIdAsync(user.Id, -1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Workout ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserWorkoutById_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);
        workout.Id = 1;

        workoutRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await workoutService.GetUserWorkoutByIdAsync(user.Id, workout.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get workout", result.ErrorMessage);
    }




    [Fact]
    public async Task UpdateUserWorkout_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        workout.Name = "New Name";

        // Act
        var result = await workoutService.UpdateUserWorkoutAsync(user.Id, workout);

        // Assert
        Assert.True(result.Success);

        var resultById = await workoutService.GetUserWorkoutByIdAsync(user.Id, workout.Id);
        Assert.NotNull(resultById.Model);
        Assert.Equal(workout.Name, resultById.Model!.Name);
    }

    [Fact]
    public async Task UpdateUserWorkout_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        workout.Name = "New Name";

        // Act
        var result = await workoutService.UpdateUserWorkoutAsync(null!, workout);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserWorkout_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        workout.Name = "New Name";

        // Act
        var result = await workoutService.UpdateUserWorkoutAsync(nonExistenceUserId, workout);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserWorkout_ShouldReturnFail_WhenWorkoutIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await workoutService.UpdateUserWorkoutAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Workout entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserWorkout_ShouldReturnFail_WhenInvalidWorkoutID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        workout.Id = -1;
        workout.Name = "New Name";

        // Act
        var result = await workoutService.UpdateUserWorkoutAsync(user.Id, workout);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Workout ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserWorkout_ShouldReturnFail_WhenWorkoutNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);

        workout.Id = 1;
        workout.Name = "New Name";

        // Act
        var result = await workoutService.UpdateUserWorkoutAsync(user.Id, workout);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Workout not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserWorkout_ShouldReturnFail_WhenAccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);

        workout.Id = 1;
        workout.Name = "New Name";
        workout.UserId = Guid.NewGuid().ToString();

        workoutRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => workout);

        // Act
        var result = await workoutService.UpdateUserWorkoutAsync(user.Id, workout);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User does not have permission to update this workout entry", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserWorkout_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);

        workout.Id = 1;
        workout.UserId = user.Id;

        workoutRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => workout);

        workoutRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Workout>()))
            .ThrowsAsync(new Exception("Database error"));

        workout.Name = "New Name";

        // Act
        var result = await workoutService.UpdateUserWorkoutAsync(user.Id, workout);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to update workout", result.ErrorMessage);
    }



    [Fact]
    public async Task UserWorkoutExists_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutService.UserWorkoutExistsAsync(user.Id, workout.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserWorkoutExists_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await workoutService.UserWorkoutExistsAsync(user.Id, 1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserWorkoutExists_ShouldThrowException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await workoutService.UserWorkoutExistsAsync(null!, workout.Id));
        Assert.Contains("User ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task UserWorkoutExists_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await workoutService.UserWorkoutExistsAsync(nonExistenceUserId, workout.Id));
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task UserWorkoutExists_ShouldThrowException_WhenInvalidWorkoutID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        workout.Id = -1;

        //Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidIDException>(async () => await workoutService.UserWorkoutExistsAsync(user.Id, workout.Id));
        Assert.Contains("Invalid Workout ID.", ex.Message);
    }

    [Fact]
    public async Task UserWorkoutExists_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);

        workout.Id = 1;

        workoutRepositoryMock
            .Setup(repo => repo.ExistsAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await workoutService.UserWorkoutExistsAsync(user.Id, workout.Id));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task UserWorkoutExistsByName_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutService.UserWorkoutExistsByNameAsync(user.Id, workout.Name);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task UserWorkoutExistsByName_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);
        string nonExistenceName = "Non-existence name";

        // Act
        var result = await workoutService.UserWorkoutExistsByNameAsync(user.Id, nonExistenceName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task UserWorkoutExistsByName_ShouldThrowException_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await workoutService.UserWorkoutExistsByNameAsync(null!, workout.Name));
        Assert.Contains("User ID cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task UserWorkoutExistsByName_ShouldThrowException_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await workoutService.UserWorkoutExistsByNameAsync(nonExistenceUserId, workout.Name));
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task UserWorkoutExistsByName_ShouldThrowException_WhenInvalidWorkoutID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);

        var user = await GetDefaultUserAsync(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await workoutService.UserWorkoutExistsByNameAsync(user.Id, null!));
        Assert.Contains("Workout name cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task UserWorkoutExistsByName_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepositoryMock = new Mock<WorkoutRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var workoutService = new WorkoutService(workoutRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var workout = await GetValidWorkoutAsync(db);

        workout.Id = 1;

        workoutRepositoryMock
            .Setup(repo => repo.ExistsByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await workoutService.UserWorkoutExistsByNameAsync(user.Id, workout.Name));
        Assert.Equal("Database error", ex.Message);
    }
}
