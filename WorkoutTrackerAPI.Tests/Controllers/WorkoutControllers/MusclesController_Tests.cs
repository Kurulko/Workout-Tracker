using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.MuscleServices;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Controllers.WorkoutControllers;

public class MusclesController_Tests : BaseWorkoutController_Tests<Muscle>
{
    static IMuscleService GetMuscleService(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new MuscleService(muscleRepository, userRepository);
    }

    static MusclesController GetMusclesController(WorkoutDbContext db)
    {
        var muscleService = GetMuscleService(db);
        return new MusclesController(muscleService);
    }

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


    [Fact]
    public async Task GetMusclesAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscles = GetValidMuscles();

        foreach (var muscle in muscles)
        {
            await muscleService.AddMuscleAsync(muscle);
        }

        // Act
        var result = await musclesController.GetMusclesAsync();

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<Muscle>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(muscles.Count(), okResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetMusclesAsync_ShouldReturnFilteredResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscles = GetValidMuscles();
        foreach (var muscle in muscles)
        {
            await muscleService.AddMuscleAsync(muscle);
        }

        // Act
        var result = await musclesController.GetMusclesAsync(pageSize: 2);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<Muscle>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.Data.Count());
    }

    [Fact]
    public async Task GetMusclesAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        // Act
        var result = await musclesController.GetMusclesAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMusclesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleService = new Mock<IMuscleService>();
        var musclesController = new MusclesController(mockMuscleService.Object);

        mockMuscleService
            .Setup(x => x.GetMusclesAsync())
            .ReturnsAsync(ServiceResult<IQueryable<Muscle>>.Fail("Failed to get muscles."));

        // Act
        var result = await musclesController.GetMusclesAsync();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get muscles.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMusclesAsync_ShouldReturnBadRequest_WhenMusclesNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleService = new Mock<IMuscleService>();
        var musclesController = new MusclesController(mockMuscleService.Object);

        mockMuscleService
            .Setup(x => x.GetMusclesAsync())
            .ReturnsAsync(ServiceResult<IQueryable<Muscle>>.Ok(null));

        // Act
        var result = await musclesController.GetMusclesAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Muscles not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task GetMuscleByIdAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        // Act
        var result = await musclesController.GetMuscleByIdAsync(-1);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Muscle ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMuscleByIdAsync_ShouldReturnMuscleById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await musclesController.GetMuscleByIdAsync(muscle.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<Muscle>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(muscle, okResult.Value);
    }

    [Fact]
    public async Task GetMuscleByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockMuscleService = new Mock<IMuscleService>();
        var musclesController = new MusclesController(mockMuscleService.Object);

        mockMuscleService
            .Setup(x => x.GetMuscleByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<Muscle>.Fail("Failed to get muscle by ID."));

        var defaultID = 1;

        // Act
        var result = await musclesController.GetMuscleByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get muscle by ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMuscleByIdAsync_ShouldReturnBadRequest_WhenMuscleNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        var notFoundID = 1;

        // Act
        var result = await musclesController.GetMuscleByIdAsync(notFoundID);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Muscle not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task AddMuscleAsync_ShouldCreateMuscle_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();

        // Act
        var result = await musclesController.AddMuscleAsync(muscle);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(MusclesController.GetMuscleByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.Equal(muscle, createdAtActionResult.Value);
    }

    [Fact]
    public async Task AddMuscleAsync_ShouldReturnBadRequest_WhenMuscleIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        // Act
        var result = await musclesController.AddMuscleAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Muscle entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddMuscleAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        muscle.Id = 1; //Invalid ID

        // Act
        var result = await musclesController.AddMuscleAsync(muscle);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Muscle ID must not be set when adding a new muscle.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddMuscleAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleService = new Mock<IMuscleService>();
        var musclesController = new MusclesController(mockMuscleService.Object);

        mockMuscleService
            .Setup(x => x.AddMuscleAsync(It.IsAny<Muscle>()))
            .ReturnsAsync(ServiceResult<Muscle>.Fail("Failed to add muscle."));

        var muscle = GetValidMuscle();

        // Act
        var result = await musclesController.AddMuscleAsync(muscle);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to add muscle.", badRequestResult.Value);
    }


    [Fact]
    public async Task UpdateMuscleAsync_ShouldUpdateMuscle_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        muscle.Name = "New Name";

        // Act
        var result = await musclesController.UpdateMuscleAsync(muscle.Id, muscle);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task UpdateMuscleAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        muscle.Id = -1; //Invalid ID
        muscle.Name = "New Name";

        // Act
        var result = await musclesController.UpdateMuscleAsync(muscle.Id, muscle);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Muscle ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateMuscleAsync_ShouldReturnBadRequest_WhenMuscleIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await musclesController.UpdateMuscleAsync(muscle.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Muscle entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateMuscleAsync_ShouldReturnBadRequest_WhenIDsDoNotMatch()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await musclesController.UpdateMuscleAsync(muscle.Id + 1, muscle);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Muscle IDs do not match.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateMuscleAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleService = new Mock<IMuscleService>();
        var musclesController = new MusclesController(mockMuscleService.Object);

        mockMuscleService
            .Setup(x => x.UpdateMuscleAsync(It.IsAny<Muscle>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to update muscle."));

        var muscle = GetValidMuscle();
        muscle.Id = 1;

        // Act
        var result = await musclesController.UpdateMuscleAsync(muscle.Id, muscle);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to update muscle.", badRequestResult.Value);
    }



    [Fact]
    public async Task DeleteMuscleUserAsync_ShouldDeleteMuscle_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await musclesController.DeleteMuscleAsync(muscle.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task DeleteMuscleAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        long invalidMuscleId = -1;

        // Act
        var result = await musclesController.DeleteMuscleAsync(invalidMuscleId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid Muscle ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteMuscleAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleService = new Mock<IMuscleService>();
        var musclesController = new MusclesController(mockMuscleService.Object);

        mockMuscleService
            .Setup(x => x.DeleteMuscleAsync(It.IsAny<long>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to delete muscle."));

        var muscle = GetValidMuscle();
        muscle.Id = 1;

        // Act
        var result = await musclesController.DeleteMuscleAsync(muscle.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to delete muscle.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMuscleByNameAsync_ShouldReturnMuscleByName_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await musclesController.GetMuscleByNameAsync(muscle.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<Muscle>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(muscle, okResult.Value);
    }

    [Fact]
    public async Task GetMuscleByNameAsync_ShouldReturnBadRequest_WhenInvalidMuscleId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        // Act
        var result = await musclesController.GetMuscleByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Muscle name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMuscleByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockMuscleService = new Mock<IMuscleService>();
        var musclesController = new MusclesController(mockMuscleService.Object);

        mockMuscleService
            .Setup(x => x.GetMuscleByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<Muscle>.Fail("Failed to get muscle by name."));

        var defaultExeciseName = "defaultExeciseName";

        // Act
        var result = await musclesController.GetMuscleByNameAsync(defaultExeciseName);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get muscle by name.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMuscleByNameAsync_ShouldReturnBadRequest_WhenMuscleNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        var notFoundExeciseName = "notFoundExeciseName";

        // Act
        var result = await musclesController.GetMuscleByNameAsync(notFoundExeciseName);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Muscle not found.", badRequestResult.Value);
    }

    [Fact]
    public async Task MuscleExistsAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        long invalidID = -1;

        // Act
        var result = await musclesController.MuscleExistsAsync(invalidID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Muscle ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task MuscleExistsAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await musclesController.MuscleExistsAsync(muscle.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task MuscleExistsAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var notFoundMuscleID = 1;

        // Act
        var result = await musclesController.MuscleExistsAsync(notFoundMuscleID);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task MuscleExistsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockMuscleService = new Mock<IMuscleService>();
        var musclesController = new MusclesController(mockMuscleService.Object);

        mockMuscleService
            .Setup(x => x.MuscleExistsAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultID = 1;

        // Act
        var result = await musclesController.MuscleExistsAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }


    [Fact]
    public async Task MuscleExistsByNameAsync_ShouldReturnBadRequest_WhenMuscleNameIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var musclesController = GetMusclesController(db);

        // Act
        var result = await musclesController.MuscleExistsByNameAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Muscle name is null or empty.", badRequestResult.Value);
    }

    [Fact]
    public async Task MuscleExistsByNameAsync_ShouldReturnTrue_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await musclesController.MuscleExistsByNameAsync(muscle.Name);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.True(okResult.Value);
    }

    [Fact]
    public async Task MuscleExistsByNameAsync_ShouldReturnFalse_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);
        var musclesController = GetMusclesController(db);

        var notFoundMuscleName = "notFoundMuscleName";

        // Act
        var result = await musclesController.MuscleExistsByNameAsync(notFoundMuscleName);

        // Assert
        var okResult = Assert.IsType<ActionResult<bool>>(result);
        Assert.False(okResult.Value);
    }

    [Fact]
    public async Task MuscleExistsByNameAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockMuscleService = new Mock<IMuscleService>();
        var musclesController = new MusclesController(mockMuscleService.Object);

        mockMuscleService
            .Setup(x => x.MuscleExistsByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        var defaultMuscleName = "defaultMuscleName";

        // Act
        var result = await musclesController.MuscleExistsByNameAsync(defaultMuscleName);

        // Assert
        var badRequestResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal("Internal server error: Database error", badRequestResult.Value);
        Assert.Equal(500, badRequestResult.StatusCode);
    }
}

