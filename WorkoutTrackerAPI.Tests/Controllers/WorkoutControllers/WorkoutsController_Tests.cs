using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.WorkoutServices;
using WorkoutTrackerAPI.Services;
using Xunit;
using WorkoutTrackerAPI.Initializers;

namespace WorkoutTrackerAPI.Tests.Controllers.WorkoutControllers;

public class WorkoutsController_Tests : BaseWorkoutController_Tests<Workout>
{
    static IWorkoutService GetWorkoutService(WorkoutDbContext db)
    {
        var workoutRepository = new WorkoutRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new WorkoutService(workoutRepository, userRepository);
    }

    WorkoutsController GetWorkoutsController(WorkoutDbContext db)
    {
        var workoutService = GetWorkoutService(db);
        return new WorkoutsController(workoutService, mockHttpContextAccessor.Object);
    }

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


    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workouts = await GetValidWorkoutsAsync(db);
        foreach (var workout in workouts)
        {
            await workoutService.AddUserWorkoutAsync(user.Id, workout);
        }

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutsAsync();

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<Workout>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(okResult.Value.TotalCount, workouts.Count());
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ShouldReturnFilteredResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workouts = await GetValidWorkoutsAsync(db);
        foreach (var workout in workouts)
        {
            await workoutService.AddUserWorkoutAsync(user.Id, workout);
        }

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutsAsync(pageSize: 2);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<Workout>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutsAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockWorkoutService = new Mock<IWorkoutService>();
        var workoutsController = new WorkoutsController(mockWorkoutService.Object, mockHttpContextAccessor.Object);

        mockWorkoutService
            .Setup(x => x.GetUserWorkoutsAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<IQueryable<Workout>>.Fail("Failed to get user workouts."));

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutsAsync();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user workouts.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutsAsync_ShouldReturnBadRequest_WhenWorkoutsNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockWorkoutService = new Mock<IWorkoutService>();
        var workoutsController = new WorkoutsController(mockWorkoutService.Object, mockHttpContextAccessor.Object);

        mockWorkoutService
            .Setup(x => x.GetUserWorkoutsAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<IQueryable<Workout>>.Ok(null));

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutsAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Workouts not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task GetCurrentUserWorkoutByIdAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        long invalidID = -1;

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutByIdAsync(invalidID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Workout ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutByIdAsync_ShouldReturnWorkoutById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutByIdAsync(workout.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<Workout>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(workout, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockWorkoutService = new Mock<IWorkoutService>();
        var workoutsController = new WorkoutsController(mockWorkoutService.Object, mockHttpContextAccessor.Object);

        mockWorkoutService
            .Setup(x => x.GetUserWorkoutByIdAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<Workout>.Fail("Failed to get user workout by ID."));

        var defaultID = 1;

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user workout by ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutByIdAsync_ShouldReturnBadRequest_WhenWorkoutNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundID = 1;

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutByIdAsync(notFoundID);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Workout not found.", notFoundResult.Value);
    }



    [Fact]
    public async Task GetCurrentUserWorkoutByNameAsync_ShouldReturnWorkoutByName_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutByNameAsync(workout.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<Workout>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(workout, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutByNameAsync_ShouldReturnBadRequest_WhenInvalidWorkoutId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Workout name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockWorkoutService = new Mock<IWorkoutService>();
        var workoutsController = new WorkoutsController(mockWorkoutService.Object, mockHttpContextAccessor.Object);

        mockWorkoutService
            .Setup(x => x.GetUserWorkoutByNameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<Workout>.Fail("Failed to get user workout by name."));

        var defaultExeciseName = "defaultExeciseName";

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutByNameAsync(defaultExeciseName);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user workout by name.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserWorkoutByNameAsync_ShouldReturnBadRequest_WhenWorkoutNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundExeciseName = "notFoundExeciseName";

        // Act
        var result = await workoutsController.GetCurrentUserWorkoutByNameAsync(notFoundExeciseName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Workout not found.", notFoundResult.Value);
    }


    [Fact]
    public async Task AddCurrentUserWorkoutAsync_ShouldCreateWorkout_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);

        // Act
        var result = await workoutsController.AddCurrentUserWorkoutAsync(workout);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(WorkoutsController.GetCurrentUserWorkoutByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.Equal(workout, createdAtActionResult.Value);
    }

    [Fact]
    public async Task AddCurrentUserWorkoutAsync_ShouldReturnBadRequest_WhenWorkoutIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        // Act
        var result = await workoutsController.AddCurrentUserWorkoutAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Workout entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddCurrentUserWorkoutAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        var workout = await GetValidWorkoutAsync(db);
        workout.Id = 1; //Invalid ID

        // Act
        var result = await workoutsController.AddCurrentUserWorkoutAsync(workout);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Workout ID must not be set when adding a new workout.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddCurrentUserWorkoutAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockWorkoutService = new Mock<IWorkoutService>();
        var workoutsController = new WorkoutsController(mockWorkoutService.Object, mockHttpContextAccessor.Object);

        mockWorkoutService
            .Setup(x => x.AddUserWorkoutAsync(It.IsAny<string>(), It.IsAny<Workout>()))
            .ReturnsAsync(ServiceResult<Workout>.Fail("Failed to add workout to user."));

        var workout = await GetValidWorkoutAsync(db);

        // Act
        var result = await workoutsController.AddCurrentUserWorkoutAsync(workout);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to add workout to user.", badRequestResult.Value);
    }


    [Fact]
    public async Task UpdateCurrentUserWorkoutAsync_ShouldUpdateWorkout_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        workout.Name = "New Name";

        // Act
        var result = await workoutsController.UpdateCurrentUserWorkoutAsync(workout.Id, workout);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task UpdateCurrentUserWorkoutAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        workout.Id = -1; //Invalid ID
        workout.Name = "New Name";

        // Act
        var result = await workoutsController.UpdateCurrentUserWorkoutAsync(workout.Id, workout);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Workout ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserWorkoutAsync_ShouldReturnBadRequest_WhenWorkoutIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutsController.UpdateCurrentUserWorkoutAsync(workout.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserWorkoutAsync_ShouldReturnBadRequest_WhenIDsDoNotMatch()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutsController.UpdateCurrentUserWorkoutAsync(workout.Id + 1, workout);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Workout IDs do not match.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserWorkoutAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockWorkoutService = new Mock<IWorkoutService>();
        var workoutsController = new WorkoutsController(mockWorkoutService.Object, mockHttpContextAccessor.Object);

        mockWorkoutService
            .Setup(x => x.UpdateUserWorkoutAsync(It.IsAny<string>(), It.IsAny<Workout>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to update user workout."));

        var workout = await GetValidWorkoutAsync(db);
        workout.Id = 1;

        // Act
        var result = await workoutsController.UpdateCurrentUserWorkoutAsync(workout.Id, workout);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to update user workout.", badRequestResult.Value);
    }



    [Fact]
    public async Task DeleteCurrentUserWorkoutAsync_ShouldDeleteWorkout_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutsController.DeleteCurrentUserWorkoutAsync(workout.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task DeleteCurrentUserWorkoutAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        long invalidWorkoutId = -1;

        // Act
        var result = await workoutsController.DeleteCurrentUserWorkoutAsync(invalidWorkoutId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Workout ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteCurrentUserWorkoutAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockWorkoutService = new Mock<IWorkoutService>();
        var workoutsController = new WorkoutsController(mockWorkoutService.Object, mockHttpContextAccessor.Object);

        mockWorkoutService
            .Setup(x => x.DeleteUserWorkoutAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to delete user workout."));

        var workout = await GetValidWorkoutAsync(db);
        workout.Id = 1;

        // Act
        var result = await workoutsController.DeleteCurrentUserWorkoutAsync(workout.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to delete user workout.", badRequestResult.Value);
    }

    [Fact]
    public async Task CurrentUserWorkoutExistsAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        long invalidID = -1;

        // Act
        var result = await workoutsController.CurrentUserWorkoutExistsAsync(invalidID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Workout ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task CurrentUserWorkoutExistsAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutsController.CurrentUserWorkoutExistsAsync(workout.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task CurrentUserWorkoutExistsAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundWorkoutID = 1;

        // Act
        var result = await workoutsController.CurrentUserWorkoutExistsAsync(notFoundWorkoutID);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task CurrentUserWorkoutExistsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockWorkoutService = new Mock<IWorkoutService>();
        var workoutsController = new WorkoutsController(mockWorkoutService.Object, mockHttpContextAccessor.Object);

        mockWorkoutService
            .Setup(x => x.UserWorkoutExistsAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultID = 1;

        // Act
        var result = await workoutsController.CurrentUserWorkoutExistsAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task CurrentUserWorkoutExistsByNameAsync_ShouldReturnBadRequest_WhenWorkoutNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutsController = GetWorkoutsController(db);

        // Act
        var result = await workoutsController.CurrentUserWorkoutExistsByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Workout name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task CurrentUserWorkoutExistsByNameAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var workout = await GetValidWorkoutAsync(db);
        await workoutService.AddUserWorkoutAsync(user.Id, workout);

        // Act
        var result = await workoutsController.CurrentUserWorkoutExistsByNameAsync(workout.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task CurrentUserWorkoutExistsByNameAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutService = GetWorkoutService(db);
        var workoutsController = GetWorkoutsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundWorkoutName = "notFoundWorkoutName";

        // Act
        var result = await workoutsController.CurrentUserWorkoutExistsByNameAsync(notFoundWorkoutName);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task CurrentUserWorkoutExistsByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockWorkoutService = new Mock<IWorkoutService>();
        var workoutsController = new WorkoutsController(mockWorkoutService.Object, mockHttpContextAccessor.Object);

        mockWorkoutService
            .Setup(x => x.UserWorkoutExistsByNameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultWorkoutName = "defaultWorkoutName";

        // Act
        var result = await workoutsController.CurrentUserWorkoutExistsByNameAsync(defaultWorkoutName);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }
}
