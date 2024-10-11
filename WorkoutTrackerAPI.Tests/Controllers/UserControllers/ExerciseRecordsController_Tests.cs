using Microsoft.AspNetCore.Mvc;
using Moq;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;
using WorkoutTrackerAPI.Services;
using Xunit;
using WorkoutTrackerAPI.Controllers.UserControllers;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Initializers;

namespace WorkoutTrackerAPI.Tests.Controllers.UserControllers;

public class ExerciseRecordsController_Tests : DbModelController_Tests<ExerciseRecord>
{
    static IExerciseRecordService GetExerciseRecordService(WorkoutDbContext db)
    {
        var exerciseRecordRepository = new ExerciseRecordRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new ExerciseRecordService(exerciseRecordRepository, userRepository);
    }

    ExerciseRecordsController GetExerciseRecordsController(WorkoutDbContext db)
    {
        var exerciseRecordService = GetExerciseRecordService(db);
        return new ExerciseRecordsController(exerciseRecordService, mockHttpContextAccessor.Object);
    }

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
        Exercise exercise1 = await GetPullUpExerciseAsync(db);
        Exercise exercise2 = await GetPlankExerciseAsync(db);

        var validExerciseRecords = new[]
             {
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-15)),
                    Reps = 10,
                    SumOfReps = 10,
                    CountOfTimes = 1,
                    ExerciseId = exercise1.Id
                },
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-25)),
                    Reps = 19,
                    SumOfReps = 29,
                    CountOfTimes = 2,
                    ExerciseId = exercise1.Id
                },
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Reps = 20,
                    SumOfReps = 49,
                    CountOfTimes = 3,
                    ExerciseId = exercise1.Id
                },
                new ExerciseRecord()
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    Time = new TimeSpan(0, 1, 0),
                    SumOfTime= new TimeSpan(0, 1, 0),
                    CountOfTimes = 1,
                    ExerciseId = exercise2.Id
                }
            };

        return validExerciseRecords;
    }


    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var exerciseRecords = await GetValidExerciseRecordsAsync(db);
        var user = await GetDefaultUserAsync(db);

        SetupMockHttpContextAccessor(user.Id);

        foreach (var exerciseRecord in exerciseRecords)
        {
            await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);
        }

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordsAsync(pullUpExercise.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<ExerciseRecord>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.All(okResult.Value.Data, m => Assert.True(m.ExerciseId == pullUpExercise.Id));
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnFilteredResult_WhenValidRequest()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exerciseRecords = await GetValidExerciseRecordsAsync(db);
        foreach (var exerciseRecord in exerciseRecords)
        {
            await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);
        }

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordsAsync(pullUpExercise.Id, pageSize: 2);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<ExerciseRecord>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.NotNull(okResult.Value);
        Assert.All(okResult.Value.Data, m => Assert.True(m.ExerciseId == pullUpExercise.Id));
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnBadRequest_WhenInvalidExerciseID()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordsAsync(-1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordsAsync(pullUpExercise.Id, pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var mockExerciseRecordService = new Mock<IExerciseRecordService>();
        var exerciseRecordsController = new ExerciseRecordsController(mockExerciseRecordService.Object, mockHttpContextAccessor.Object);

        mockExerciseRecordService
            .Setup(x => x.GetUserExerciseRecordsAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<IQueryable<ExerciseRecord>>.Fail("Failed to get user exercise records."));

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordsAsync(pullUpExercise.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user exercise records.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordsAsync_ShouldReturnBadRequest_WhenExerciseRecordsNotFound()
    {
        using var db = contextFactory.CreateDatabaseContext();
        var mockExerciseRecordService = new Mock<IExerciseRecordService>();
        var exerciseRecordsController = new ExerciseRecordsController(mockExerciseRecordService.Object, mockHttpContextAccessor.Object);

        mockExerciseRecordService
            .Setup(x => x.GetUserExerciseRecordsAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<IQueryable<ExerciseRecord>>.Ok(null));

        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordsAsync(pullUpExercise.Id);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Exercise records not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task GetCurrentUserExerciseRecordByIdAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        long invalidID = -1;

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordByIdAsync(invalidID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid ExerciseRecord ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordByIdAsync_ShouldReturnExerciseRecordById_WhenValidRequest()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordByIdAsync(exerciseRecord.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<ExerciseRecord>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(exerciseRecord, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseRecordService = new Mock<IExerciseRecordService>();
        var exerciseRecordsController = new ExerciseRecordsController(mockExerciseRecordService.Object, mockHttpContextAccessor.Object);

        mockExerciseRecordService
            .Setup(x => x.GetUserExerciseRecordByIdAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<ExerciseRecord>.Fail("Failed to get user exercise record by ID."));

        var defaultID = 1;

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user exercise record by ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordByIdAsync_ShouldReturnBadRequest_WhenExerciseRecordNotFound()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundID = 1;

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordByIdAsync(notFoundID);

        // Assert
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Exercise record not found.", notFoundObjectResult.Value);
    }



    [Fact]
    public async Task GetCurrentUserExerciseRecordByDateAsync_ShouldReturnExerciseRecordByDate_WhenValidRequest()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordByDateAsync(exerciseRecord.ExerciseId, exerciseRecord.Date.ToDateTime(new TimeOnly()));

        // Assert
        var okResult = Assert.IsType<ActionResult<ExerciseRecord>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(exerciseRecord, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordByDateAsync_ShouldReturnBadRequest_WhenInvalidExerciseId()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        long invalidExerciseID = -1;
        var defaultDateTime = DateTime.Now;

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordByDateAsync(invalidExerciseID, defaultDateTime);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Exercise ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordByDateAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockExerciseRecordService = new Mock<IExerciseRecordService>();
        var exerciseRecordsController = new ExerciseRecordsController(mockExerciseRecordService.Object, mockHttpContextAccessor.Object);

        mockExerciseRecordService
            .Setup(x => x.GetUserExerciseRecordByDateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(ServiceResult<ExerciseRecord>.Fail("Failed to get user exercise record by date."));

        var defaultExeciseID = 1;
        var defaultDateTime = DateTime.Now;

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordByDateAsync(defaultExeciseID, defaultDateTime);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user exercise record by date.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserExerciseRecordByDateAsync_ShouldReturnBadRequest_WhenExerciseRecordNotFound()
    {
        //Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundDateTime = DateTime.Now.AddDays(-1111);
        var pullUpExercise = await GetPullUpExerciseAsync(db);

        // Act
        var result = await exerciseRecordsController.GetCurrentUserExerciseRecordByDateAsync(pullUpExercise.Id, notFoundDateTime);

        // Assert
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Exercise record not found.", notFoundObjectResult.Value);
    }


    [Fact]
    public async Task AddExerciseRecordToCurrentUserAsync_ShouldCreateExerciseRecord_WhenValidRequest()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        // Act
        var result = await exerciseRecordsController.AddExerciseRecordToCurrentUserAsync(exerciseRecord);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ExerciseRecordsController.GetCurrentUserExerciseRecordByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.Equal(exerciseRecord, createdAtActionResult.Value);
    }

    [Fact]
    public async Task AddExerciseRecordToCurrentUserAsync_ShouldReturnBadRequest_WhenExerciseRecordIsNull()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        // Act
        var result = await exerciseRecordsController.AddExerciseRecordToCurrentUserAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Exercise record entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddExerciseRecordToCurrentUserAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        exerciseRecord.Id = 1; //Invalid ID

        // Act
        var result = await exerciseRecordsController.AddExerciseRecordToCurrentUserAsync(exerciseRecord);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("ExerciseRecord ID must not be set when adding a new exercise record.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddExerciseRecordToCurrentUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockExerciseRecordService = new Mock<IExerciseRecordService>();
        var exerciseRecordsController = new ExerciseRecordsController(mockExerciseRecordService.Object, mockHttpContextAccessor.Object);

        mockExerciseRecordService
            .Setup(x => x.AddExerciseRecordToUserAsync(It.IsAny<string>(), It.IsAny<ExerciseRecord>()))
            .ReturnsAsync(ServiceResult<ExerciseRecord>.Fail("Failed to add exercise record to user."));

        var exerciseRecord = await GetValidExerciseRecordAsync(db);

        // Act
        var result = await exerciseRecordsController.AddExerciseRecordToCurrentUserAsync(exerciseRecord);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to add exercise record to user.", badRequestResult.Value);
    }


    [Fact]
    public async Task UpdateCurrentUserExerciseRecordAsync_ShouldUpdateExerciseRecord_WhenValidRequest()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        exerciseRecord.Weight = 78;

        // Act
        var result = await exerciseRecordsController.UpdateCurrentUserExerciseRecordAsync(exerciseRecord.Id, exerciseRecord);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task UpdateCurrentUserExerciseRecordAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        exerciseRecord.Id = -1; //Invalid ID
        exerciseRecord.Reps = 21;

        // Act
        var result = await exerciseRecordsController.UpdateCurrentUserExerciseRecordAsync(exerciseRecord.Id, exerciseRecord);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid ExerciseRecord ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserExerciseRecordAsync_ShouldReturnBadRequest_WhenExerciseRecordIsNull()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        // Act
        var result = await exerciseRecordsController.UpdateCurrentUserExerciseRecordAsync(exerciseRecord.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Exercise record entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserExerciseRecordAsync_ShouldReturnBadRequest_WhenIDsDoNotMatch()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        // Act
        var result = await exerciseRecordsController.UpdateCurrentUserExerciseRecordAsync(exerciseRecord.Id + 1, exerciseRecord);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("ExerciseRecord IDs do not match.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserExerciseRecordAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockExerciseRecordService = new Mock<IExerciseRecordService>();
        var exerciseRecordsController = new ExerciseRecordsController(mockExerciseRecordService.Object, mockHttpContextAccessor.Object);

        mockExerciseRecordService
            .Setup(x => x.UpdateUserExerciseRecordAsync(It.IsAny<string>(), It.IsAny<ExerciseRecord>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to update user exercise record."));

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        exerciseRecord.Id = 1;

        // Act
        var result = await exerciseRecordsController.UpdateCurrentUserExerciseRecordAsync(exerciseRecord.Id, exerciseRecord);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to update user exercise record.", badRequestResult.Value);
    }



    [Fact]
    public async Task DeleteExerciseRecordFromCurrentUserAsync_ShouldDeleteExerciseRecord_WhenValidRequest()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordService = GetExerciseRecordService(db);
        var exerciseRecordsController = GetExerciseRecordsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        await exerciseRecordService.AddExerciseRecordToUserAsync(user.Id, exerciseRecord);

        // Act
        var result = await exerciseRecordsController.DeleteExerciseRecordFromCurrentUserAsync(exerciseRecord.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task DeleteExerciseRecordFromCurrentUserAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var exerciseRecordsController = GetExerciseRecordsController(db);

        long invalidExerciseRecordId = -1;

        // Act
        var result = await exerciseRecordsController.DeleteExerciseRecordFromCurrentUserAsync(invalidExerciseRecordId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid ExerciseRecord ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteExerciseRecordFromCurrentUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = contextFactory.CreateDatabaseContext();
        var mockExerciseRecordService = new Mock<IExerciseRecordService>();
        var exerciseRecordsController = new ExerciseRecordsController(mockExerciseRecordService.Object, mockHttpContextAccessor.Object);

        mockExerciseRecordService
            .Setup(x => x.DeleteExerciseRecordFromUserAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to delete user exercise record."));

        var exerciseRecord = await GetValidExerciseRecordAsync(db);
        exerciseRecord.Id = 1;

        // Act
        var result = await exerciseRecordsController.DeleteExerciseRecordFromCurrentUserAsync(exerciseRecord.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to delete user exercise record.", badRequestResult.Value);
    }
}
