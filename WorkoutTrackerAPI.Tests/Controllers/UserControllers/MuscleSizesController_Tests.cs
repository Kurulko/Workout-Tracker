using Microsoft.AspNetCore.Mvc;
using Moq;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.MuscleSizeServices;
using WorkoutTrackerAPI.Services;
using Xunit;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Initializers;

namespace WorkoutTrackerAPI.Tests.Controllers.UserControllers;

public class MuscleSizesController_Tests : DbModelController_Tests<MuscleSize>
{
    static IMuscleSizeService GetMuscleSizeService(WorkoutDbContext db)
    {
        var muscleSizeRepository = new MuscleSizeRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new MuscleSizeService(muscleSizeRepository, userRepository);
    }

    MuscleSizesController GetMuscleSizesController(WorkoutDbContext db)
    {
        var muscleSizeService = GetMuscleSizeService(db);
        return new MuscleSizesController(muscleSizeService, mockHttpContextAccessor.Object);
    }

    static async Task<Muscle> GetBackMuscleAsync(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);

        string name = "Back";
        var muscle = await muscleRepository.GetByNameAsync(name);

        if (muscle is null)
        {
            MuscleData muscleData = new()
            {
                Name = name
            };
            muscle = await MusclesInitializer.InitializeAsync(muscleRepository, muscleData, null);
        }

        return muscle;
    }

    static async Task<Muscle> GetBicepsMuscleAsync(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);

        string name = "Biceps";
        var muscle = await muscleRepository.GetByNameAsync(name);

        if (muscle is null)
        {
            MuscleData muscleData = new()
            {
                Name = "Biceps"
            };
            muscle = await MusclesInitializer.InitializeAsync(muscleRepository, muscleData, null);
        }

        return muscle;
    }

    async Task<MuscleSize> GetValidMuscleSizeAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);
        Muscle muscle = await GetBicepsMuscleAsync(db);

        var validMuscleSize = new MuscleSize()
        {
            Date = DateTime.Now,
            Size = 40,
            SizeType = SizeType.Centimeter,
            MuscleId = muscle.Id,
            UserId = user.Id
        };

        return validMuscleSize;
    }

    async Task<IEnumerable<MuscleSize>> GetValidMuscleSizesAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);
        Muscle bicepsMuscle = await GetBicepsMuscleAsync(db);
        Muscle backMuscle = await GetBackMuscleAsync(db);

        var validMuscleSizes = new[]
             {
                new MuscleSize()
                {
                    Date = DateTime.Now.AddDays(-150),
                    Size = 37,
                    SizeType = SizeType.Centimeter,
                    MuscleId = bicepsMuscle.Id,
                    UserId = user.Id
                },
                new MuscleSize()
                {
                    Date = DateTime.Now,
                    Size = 40,
                    SizeType = SizeType.Centimeter,
                    MuscleId = bicepsMuscle.Id,
                    UserId = user.Id
                },
                new MuscleSize()
                {
                    Date = DateTime.Now,
                    Size = 120,
                    SizeType = SizeType.Centimeter,
                    MuscleId = backMuscle.Id,
                    UserId = user.Id
                }
            };

        return validMuscleSizes;
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizesAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        var user = await GetDefaultUserAsync(db);

        SetupMockHttpContextAccessor(user.Id);

        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizesAsync(bicepsMuscle.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<MuscleSize>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.All(okResult.Value.Data, m => Assert.True(m.MuscleId == bicepsMuscle.Id));
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizesAsync_ShouldReturnFilteredResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizesAsync(bicepsMuscle.Id, pageSize: 2);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<MuscleSize>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizesAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizesController = GetMuscleSizesController(db);

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizesAsync(bicepsMuscle.Id, pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizesAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.GetUserMuscleSizesAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<IQueryable<MuscleSize>>.Fail("Failed to get user muscle sizes."));
        
        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizesAsync(bicepsMuscle.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user muscle sizes.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizesAsync_ShouldReturnBadRequest_WhenMuscleSizesNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.GetUserMuscleSizesAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<IQueryable<MuscleSize>>.Ok(null));

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizesAsync(bicepsMuscle.Id);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Muscle sizes not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task GetCurrentUserMuscleSizeByIdAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizesController = GetMuscleSizesController(db);

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizeByIdAsync(0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid MuscleSize ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizeByIdAsync_ShouldReturnMuscleSizeById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizeByIdAsync(muscleSize.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<MuscleSize>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(muscleSize, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizeByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.GetUserMuscleSizeByIdAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<MuscleSize>.Fail("Failed to get user muscle size by ID."));

        var defaultID = 1;

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizeByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user muscle size by ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizeByIdAsync_ShouldReturnBadRequest_WhenMuscleSizeNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundID = 1;

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizeByIdAsync(notFoundID);

        // Assert
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Muscle size not found.", notFoundObjectResult.Value);
    }



    [Fact]
    public async Task GetCurrentUserMuscleSizeByDateAsync_ShouldReturnMuscleSizeByDate_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizeByDateAsync(bicepsMuscle.Id, muscleSize.Date);

        // Assert
        var okResult = Assert.IsType<ActionResult<MuscleSize>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(muscleSize, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizeByDateAsync_ShouldReturnBadRequest_WhenInvalidMuscleId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizesController = GetMuscleSizesController(db);

        long invalidMuscleID = -1;
        var defaultDateTime = DateTime.Now;

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizeByDateAsync(invalidMuscleID, defaultDateTime);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid Muscle ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizeByDateAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.GetUserMuscleSizeByDateAsync(It.IsAny<string>(), It.IsAny<long>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(ServiceResult<MuscleSize>.Fail("Failed to get user muscle size by date."));

        var defaultDateTime = DateTime.Now;
        var defaultMuscleID = 1;

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizeByDateAsync(defaultMuscleID, defaultDateTime);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user muscle size by date.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserMuscleSizeByDateAsync_ShouldReturnBadRequest_WhenMuscleSizeNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundDateTime = DateTime.Now.AddDays(-1111);
        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizesController.GetCurrentUserMuscleSizeByDateAsync(bicepsMuscle.Id, notFoundDateTime);

        // Assert
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Muscle size not found.", notFoundObjectResult.Value);
    }



    [Fact]
    public async Task GetMinCurrentUserMuscleSizeAsync_ShouldReturnMuscleSize_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizesController.GetMinCurrentUserMuscleSizeAsync(bicepsMuscle.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<MuscleSize>>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMinCurrentUserMuscleSizeAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.GetMinUserMuscleSizeAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<MuscleSize>.Fail("Failed to get user min muscle size."));

        var defaultMuscleId = 1;

        // Act
        var result = await muscleSizesController.GetMinCurrentUserMuscleSizeAsync(defaultMuscleId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user min muscle size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMinCurrentUserMuscleSizeAsync_ShouldReturnBadRequest_WhenMuscleSizeNotFound()
    {
        //Arrange
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.GetMinUserMuscleSizeAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<MuscleSize>.Ok(null));

        var defaultMuscleId = 1;

        // Act
        var result = await muscleSizesController.GetMinCurrentUserMuscleSizeAsync(defaultMuscleId);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Muscle size not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task GetMaxCurrentUserMuscleSizeAsync_ShouldReturnMuscleSize_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizesController.GetMaxCurrentUserMuscleSizeAsync(bicepsMuscle.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<MuscleSize>>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMaxCurrentUserMuscleSizeAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.GetMaxUserMuscleSizeAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<MuscleSize>.Fail("Failed to get user max muscle size."));

        var defaultMuscleId = 1;

        // Act
        var result = await muscleSizesController.GetMaxCurrentUserMuscleSizeAsync(defaultMuscleId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user max muscle size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMaxCurrentUserMuscleSizeAsync_ShouldReturnBadRequest_WhenMuscleSizeNotFound()
    {
        //Arrange
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.GetMaxUserMuscleSizeAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<MuscleSize>.Ok(null));

        var defaultMuscleId = 1;

        // Act
        var result = await muscleSizesController.GetMaxCurrentUserMuscleSizeAsync(defaultMuscleId);

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Muscle size not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task AddMuscleSizeToCurrentUserAsync_ShouldCreateMuscleSize_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSize = await GetValidMuscleSizeAsync(db);

        // Act
        var result = await muscleSizesController.AddMuscleSizeToCurrentUserAsync(muscleSize);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(MuscleSizesController.GetCurrentUserMuscleSizeByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.Equal(muscleSize, createdAtActionResult.Value);
    }

    [Fact]
    public async Task AddMuscleSizeToCurrentUserAsync_ShouldReturnBadRequest_WhenMuscleSizeIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizesController = GetMuscleSizesController(db);

        // Act
        var result = await muscleSizesController.AddMuscleSizeToCurrentUserAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Muscle size entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddMuscleSizeToCurrentUserAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizesController = GetMuscleSizesController(db);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        muscleSize.Id = 1; //Invalid ID

        // Act
        var result = await muscleSizesController.AddMuscleSizeToCurrentUserAsync(muscleSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("MuscleSize ID must not be set when adding a new muscle size.", badRequestResult.Value);
    }

    [Fact]
    public async Task AddMuscleSizeToCurrentUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.AddMuscleSizeToUserAsync(It.IsAny<string>(), It.IsAny<MuscleSize>()))
            .ReturnsAsync(ServiceResult<MuscleSize>.Fail("Failed to add muscle size to user."));

        var muscleSize = await GetValidMuscleSizeAsync(db);

        // Act
        var result = await muscleSizesController.AddMuscleSizeToCurrentUserAsync(muscleSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to add muscle size to user.", badRequestResult.Value);
    }


    [Fact]
    public async Task UpdateCurrentUserMuscleSizeAsync_ShouldUpdateMuscleSize_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        muscleSize.Size = 35;

        // Act
        var result = await muscleSizesController.UpdateCurrentUserMuscleSizeAsync(muscleSize.Id, muscleSize);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task UpdateCurrentUserMuscleSizeAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        muscleSize.Id = -1; //Invalid ID
        muscleSize.Size = 35;

        // Act
        var result = await muscleSizesController.UpdateCurrentUserMuscleSizeAsync(muscleSize.Id, muscleSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid MuscleSize ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserMuscleSizeAsync_ShouldReturnBadRequest_WhenMuscleSizeIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        // Act
        var result = await muscleSizesController.UpdateCurrentUserMuscleSizeAsync(muscleSize.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Muscle size entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserMuscleSizeAsync_ShouldReturnBadRequest_WhenIDsDoNotMatch()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        // Act
        var result = await muscleSizesController.UpdateCurrentUserMuscleSizeAsync(muscleSize.Id + 1, muscleSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("MuscleSize IDs do not match.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserMuscleSizeAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.UpdateUserMuscleSizeAsync(It.IsAny<string>(), It.IsAny<MuscleSize>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to update user muscle size."));

        var muscleSize = await GetValidMuscleSizeAsync(db);
        muscleSize.Id = 1;

        // Act
        var result = await muscleSizesController.UpdateCurrentUserMuscleSizeAsync(muscleSize.Id, muscleSize);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to update user muscle size.", badRequestResult.Value);
    }



    [Fact]
    public async Task DeleteMuscleSizeFromCurrentUserAsync_ShouldDeleteMuscleSize_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);
        var muscleSizesController = GetMuscleSizesController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        // Act
        var result = await muscleSizesController.DeleteMuscleSizeFromCurrentUserAsync(muscleSize.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task DeleteMuscleSizeFromCurrentUserAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizesController = GetMuscleSizesController(db);

        long invalidMuscleSizeId = -1;

        // Act
        var result = await muscleSizesController.DeleteMuscleSizeFromCurrentUserAsync(invalidMuscleSizeId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid MuscleSize ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteMuscleSizeFromCurrentUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockMuscleSizeService = new Mock<IMuscleSizeService>();
        var muscleSizesController = new MuscleSizesController(mockMuscleSizeService.Object, mockHttpContextAccessor.Object);

        mockMuscleSizeService
            .Setup(x => x.DeleteMuscleSizeFromUserAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to delete user muscle size."));

        var muscleSize = await GetValidMuscleSizeAsync(db);
        muscleSize.Id = 1;

        // Act
        var result = await muscleSizesController.DeleteMuscleSizeFromCurrentUserAsync(muscleSize.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to delete user muscle size.", badRequestResult.Value);
    }
}
