using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Controllers.UserControllers;
using WorkoutTracker.API.Data.Models.UserModels;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories.UserRepositories;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Services.ExerciseServices;
using Xunit;
using ExerciseTrackerAPI.Controllers.ExerciseControllers;
using WorkoutTracker.API.Services;

namespace WorkoutTracker.API.Tests.Controllers.WorkoutControllers;

public class ExercisesController_Tests : BaseWorkoutController_Tests<Exercise>
{
    static IExerciseService GetExerciseService(WorkoutDbContext db)
    {
        var exerciseRepository = new ExerciseRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new ExerciseService(exerciseRepository, userRepository);
    }

    ExercisesController GetExercisesController(WorkoutDbContext db)
    {
        var exerciseService = GetExerciseService(db);
        return new ExercisesController(exerciseService, mockHttpContextAccessor.Object);
    }

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


    [Fact]
    public async Task GetCurrentUserExercisesAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddUserExerciseAsync(user.Id, exercise);
        }

        // Act
        var result = await exercisesController.GetCurrentUserExercisesAsync();

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<Exercise>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(okResult.Value.TotalCount, exercises.Count());
    }

    [Fact]
    public async Task GetCurrentUserExercisesAsync_ShouldReturnFilteredResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddUserExerciseAsync(user.Id, exercise);
        }

        // Act
        var result = await exercisesController.GetCurrentUserExercisesAsync(pageSize: 2);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<Exercise>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.Data.Count());
    }


    [Fact]
    public async Task GetCurrentUserExercisesAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        // Act
        var result = await exercisesController.GetCurrentUserExercisesAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExercisesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.GetUserExercisesAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<IQueryable<Exercise>>.Fail("Failed to get user exercises."));

        // Act
        var result = await exercisesController.GetCurrentUserExercisesAsync();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user exercises.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExercisesAsync_ShouldReturnBadRequest_WhenExercisesNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.GetUserExercisesAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<IQueryable<Exercise>>.Ok(null));

        // Act
        var result = await exercisesController.GetCurrentUserExercisesAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User exercises not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task GetCurrentUserExerciseByIdAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        long invalidID = -1;

        // Act
        var result = await exercisesController.GetCurrentUserExerciseByIdAsync(invalidID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseByIdAsync_ShouldReturnExerciseById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exercisesController.GetCurrentUserExerciseByIdAsync(exercise.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<Exercise>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(exercise, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.GetUserExerciseByIdAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<Exercise>.Fail("Failed to get user exercise by ID."));

        var defaultID = 1;

        // Act
        var result = await exercisesController.GetCurrentUserExerciseByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user exercise by ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseByIdAsync_ShouldReturnBadRequest_WhenExerciseNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundID = 1;

        // Act
        var result = await exercisesController.GetCurrentUserExerciseByIdAsync(notFoundID);

        // Assert
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User exercise not found.", notFoundObjectResult.Value);
    }



    [Fact]
    public async Task GetCurrentUserExerciseByNameAsync_ShouldReturnExerciseByName_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exercisesController.GetCurrentUserExerciseByNameAsync(exercise.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<Exercise>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(exercise, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseByNameAsync_ShouldReturnBadRequest_WhenInvalidExerciseId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        // Act
        var result = await exercisesController.GetCurrentUserExerciseByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Exercise name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.GetUserExerciseByNameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<Exercise>.Fail("Failed to get user exercise by name."));

        var defaultExeciseName = "defaultExeciseName";

        // Act
        var result = await exercisesController.GetCurrentUserExerciseByNameAsync(defaultExeciseName);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user exercise by name.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseByNameAsync_ShouldReturnBadRequest_WhenExerciseNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundExeciseName = "notFoundExeciseName";

        // Act
        var result = await exercisesController.GetCurrentUserExerciseByNameAsync(notFoundExeciseName);

        // Assert
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("User exercise not found.", notFoundObjectResult.Value);
    }


    [Fact]
    public async Task AddCurrentUserExerciseAsync_ShouldCreateExercise_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);

        // Act
        var result = await exercisesController.AddCurrentUserExerciseAsync(exercise);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ExercisesController.GetCurrentUserExerciseByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.Equal(exercise, createdAtActionResult.Value);
    }

    [Fact]
    public async Task AddCurrentUserExerciseAsync_ShouldReturnBadRequest_WhenExerciseIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        // Act
        var result = await exercisesController.AddCurrentUserExerciseAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Exercise entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddCurrentUserExerciseAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1; //Invalid ID

        // Act
        var result = await exercisesController.AddCurrentUserExerciseAsync(exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Exercise ID must not be set when adding a new exercise.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddCurrentUserExerciseAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.AddUserExerciseAsync(It.IsAny<string>(), It.IsAny<Exercise>()))
            .ReturnsAsync(ServiceResult<Exercise>.Fail("Failed to add exercise to user."));

        var exercise = await GetValidExerciseAsync(db);

        // Act
        var result = await exercisesController.AddCurrentUserExerciseAsync(exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to add exercise to user.", badRequestResult.Value);
    }


    [Fact]
    public async Task UpdateCurrentUserExerciseAsync_ShouldUpdateExercise_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        exercise.Name = "New Name";

        // Act
        var result = await exercisesController.UpdateCurrentUserExerciseAsync(exercise.Id, exercise);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task UpdateCurrentUserExerciseAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        exercise.Id = -1; //Invalid ID
        exercise.Name = "New Name";

        // Act
        var result = await exercisesController.UpdateCurrentUserExerciseAsync(exercise.Id, exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserExerciseAsync_ShouldReturnBadRequest_WhenExerciseIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exercisesController.UpdateCurrentUserExerciseAsync(exercise.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Exercise entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserExerciseAsync_ShouldReturnBadRequest_WhenIDsDoNotMatch()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exercisesController.UpdateCurrentUserExerciseAsync(exercise.Id + 1, exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Exercise IDs do not match.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserExerciseAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.UpdateUserExerciseAsync(It.IsAny<string>(), It.IsAny<Exercise>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to update user exercise."));

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1;

        // Act
        var result = await exercisesController.UpdateCurrentUserExerciseAsync(exercise.Id, exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to update user exercise.", badRequestResult.Value);
    }



    [Fact]
    public async Task DeleteExerciseFromCurrentUserAsync_ShouldDeleteExercise_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exercisesController.DeleteExerciseFromCurrentUserAsync(exercise.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task DeleteExerciseFromCurrentUserAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        long invalidExerciseId = -1;

        // Act
        var result = await exercisesController.DeleteExerciseFromCurrentUserAsync(invalidExerciseId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteExerciseFromCurrentUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.DeleteExerciseFromUserAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to delete user exercise."));

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1;

        // Act
        var result = await exercisesController.DeleteExerciseFromCurrentUserAsync(exercise.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to delete user exercise.", badRequestResult.Value);
    }



    [Fact]
    public async Task CurrentUserExerciseExistsAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        long invalidID = -1;

        // Act
        var result = await exercisesController.CurrentUserExerciseExistsAsync(invalidID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task CurrentUserExerciseExistsAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exercisesController.CurrentUserExerciseExistsAsync(exercise.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task CurrentUserExerciseExistsAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundExerciseID = 1;

        // Act
        var result = await exercisesController.CurrentUserExerciseExistsAsync(notFoundExerciseID);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task CurrentUserExerciseExistsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.UserExerciseExistsAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultID = 1;

        // Act
        var result = await exercisesController.CurrentUserExerciseExistsAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task CurrentUserExerciseExistsByNameAsync_ShouldReturnBadRequest_WhenExerciseNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        // Act
        var result = await exercisesController.CurrentUserExerciseExistsByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Exercise name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task CurrentUserExerciseExistsByNameAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddUserExerciseAsync(user.Id, exercise);

        // Act
        var result = await exercisesController.CurrentUserExerciseExistsByNameAsync(exercise.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task CurrentUserExerciseExistsByNameAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundExerciseName = "notFoundExerciseName";

        // Act
        var result = await exercisesController.CurrentUserExerciseExistsByNameAsync(notFoundExerciseName);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task CurrentUserExerciseExistsByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.UserExerciseExistsByNameAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultExerciseName = "defaultExerciseName";

        // Act
        var result = await exercisesController.CurrentUserExerciseExistsByNameAsync(defaultExerciseName);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }









    [Fact]
    public async Task GetExercisesAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddExerciseAsync(exercise);
        }

        // Act
        var result = await exercisesController.GetExercisesAsync();

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<Exercise>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(okResult.Value.TotalCount, exercises.Count());
    }

    [Fact]
    public async Task GetExercisesAsync_ShouldReturnFilteredResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercises = await GetValidExercisesAsync(db);
        foreach (var exercise in exercises)
        {
            await exerciseService.AddExerciseAsync(exercise);
        }

        // Act
        var result = await exercisesController.GetExercisesAsync(pageSize: 2);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<Exercise>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.Data.Count());
    }


    [Fact]
    public async Task GetExercisesAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        // Act
        var result = await exercisesController.GetExercisesAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetExercisesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.GetExercisesAsync())
            .ReturnsAsync(ServiceResult<IQueryable<Exercise>>.Fail("Failed to get exercises."));

        // Act
        var result = await exercisesController.GetExercisesAsync();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get exercises.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetExercisesAsync_ShouldReturnBadRequest_WhenExercisesNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.GetExercisesAsync())
            .ReturnsAsync(ServiceResult<IQueryable<Exercise>>.Ok(null));

        // Act
        var result = await exercisesController.GetExercisesAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Exercises not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task GetExerciseByIdAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        long invalidID = -1;

        // Act
        var result = await exercisesController.GetExerciseByIdAsync(invalidID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetExerciseByIdAsync_ShouldReturnExerciseById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exercisesController.GetExerciseByIdAsync(exercise.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<Exercise>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(exercise, okResult.Value);
    }

    [Fact]
    public async Task GetExerciseByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.GetExerciseByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<Exercise>.Fail("Failed to get exercise by ID."));

        var defaultID = 1;

        // Act
        var result = await exercisesController.GetExerciseByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get exercise by ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetExerciseByIdAsync_ShouldReturnBadRequest_WhenExerciseNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        var notFoundID = 1;

        // Act
        var result = await exercisesController.GetExerciseByIdAsync(notFoundID);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Exercise not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task GetExerciseByNameAsync_ShouldReturnExerciseByName_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exercisesController.GetExerciseByNameAsync(exercise.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<Exercise>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(exercise, okResult.Value);
    }

    [Fact]
    public async Task GetExerciseByNameAsync_ShouldReturnBadRequest_WhenInvalidExerciseId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        // Act
        var result = await exercisesController.GetExerciseByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Exercise name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetExerciseByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.GetExerciseByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<Exercise>.Fail("Failed to get exercise by name."));

        var defaultExeciseName = "defaultExeciseName";

        // Act
        var result = await exercisesController.GetExerciseByNameAsync(defaultExeciseName);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get exercise by name.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetExerciseByNameAsync_ShouldReturnBadRequest_WhenExerciseNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        var notFoundExeciseName = "notFoundExeciseName";

        // Act
        var result = await exercisesController.GetExerciseByNameAsync(notFoundExeciseName);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Exercise not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task AddExerciseAsync_ShouldCreateExercise_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);

        // Act
        var result = await exercisesController.AddExerciseAsync(exercise);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ExercisesController.GetExerciseByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.Equal(exercise, createdAtActionResult.Value);
    }

    [Fact]
    public async Task AddExerciseAsync_ShouldReturnBadRequest_WhenExerciseIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        // Act
        var result = await exercisesController.AddExerciseAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Exercise entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddExerciseAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1; //Invalid ID

        // Act
        var result = await exercisesController.AddExerciseAsync(exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Exercise ID must not be set when adding a new exercise.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddExerciseAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.AddExerciseAsync(It.IsAny<Exercise>()))
            .ReturnsAsync(ServiceResult<Exercise>.Fail("Failed to add exercise."));

        var exercise = await GetValidExerciseAsync(db);

        // Act
        var result = await exercisesController.AddExerciseAsync(exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to add exercise.", badRequestResult.Value);
    }


    [Fact]
    public async Task UpdateExerciseAsync_ShouldUpdateExercise_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        exercise.Name = "New Name";

        // Act
        var result = await exercisesController.UpdateExerciseAsync(exercise.Id, exercise);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        exercise.Id = -1; //Invalid ID
        exercise.Name = "New Name";

        // Act
        var result = await exercisesController.UpdateExerciseAsync(exercise.Id, exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldReturnBadRequest_WhenExerciseIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exercisesController.UpdateExerciseAsync(exercise.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Exercise entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldReturnBadRequest_WhenIDsDoNotMatch()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exercisesController.UpdateExerciseAsync(exercise.Id + 1, exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Exercise IDs do not match.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateExerciseAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.UpdateExerciseAsync(It.IsAny<Exercise>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to update exercise."));

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1;

        // Act
        var result = await exercisesController.UpdateExerciseAsync(exercise.Id, exercise);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to update exercise.", badRequestResult.Value);
    }



    [Fact]
    public async Task DeleteExerciseAsync_ShouldDeleteExercise_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exercisesController.DeleteExerciseAsync(exercise.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task DeleteExerciseAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        long invalidExerciseId = -1;

        // Act
        var result = await exercisesController.DeleteExerciseAsync(invalidExerciseId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteExerciseAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.DeleteExerciseFromUserAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to delete user exercise."));

        var exercise = await GetValidExerciseAsync(db);
        exercise.Id = 1;

        // Act
        var result = await exercisesController.DeleteExerciseFromCurrentUserAsync(exercise.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to delete user exercise.", badRequestResult.Value);
    }



    [Fact]
    public async Task ExerciseExistsAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        long invalidID = -1;

        // Act
        var result = await exercisesController.ExerciseExistsAsync(invalidID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task ExerciseExistsAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exercisesController.ExerciseExistsAsync(exercise.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task ExerciseExistsAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var notFoundExerciseID = 1;

        // Act
        var result = await exercisesController.ExerciseExistsAsync(notFoundExerciseID);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task ExerciseExistsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.ExerciseExistsAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultID = 1;

        // Act
        var result = await exercisesController.ExerciseExistsAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task ExerciseExistsByNameAsync_ShouldReturnBadRequest_WhenExerciseNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exercisesController = GetExercisesController(db);

        // Act
        var result = await exercisesController.ExerciseExistsByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Exercise name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task ExerciseExistsByNameAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var exercise = await GetValidExerciseAsync(db);
        await exerciseService.AddExerciseAsync(exercise);

        // Act
        var result = await exercisesController.ExerciseExistsByNameAsync(exercise.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task ExerciseExistsByNameAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseService = GetExerciseService(db);
        var exercisesController = GetExercisesController(db);

        var notFoundExerciseName = "notFoundExerciseName";

        // Act
        var result = await exercisesController.ExerciseExistsByNameAsync(notFoundExerciseName);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task ExerciseExistsByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseService = new Mock<IExerciseService>();
        var exercisesController = new ExercisesController(mockExerciseService.Object, mockHttpContextAccessor.Object);

        mockExerciseService
            .Setup(x => x.ExerciseExistsByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultExerciseName = "defaultExerciseName";

        // Act
        var result = await exercisesController.ExerciseExistsByNameAsync(defaultExerciseName);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }
}

