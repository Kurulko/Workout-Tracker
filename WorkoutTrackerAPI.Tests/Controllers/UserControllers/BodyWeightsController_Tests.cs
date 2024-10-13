using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using Xunit;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Tests.Services;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.BodyWeightServices;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Tests.Controllers.UserControllers;

public class BodyWeightsController_Tests : DbModelController_Tests<BodyWeight>
{
    static IBodyWeightService GetBodyWeightService(WorkoutDbContext db)
    {
        var bodyWeightRepository = new BodyWeightRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new BodyWeightService(bodyWeightRepository, userRepository);
    }

    BodyWeightsController GetBodyWeightsController(WorkoutDbContext db)
    {
        var bodyWeightService = GetBodyWeightService(db);
        return new BodyWeightsController(bodyWeightService, mockHttpContextAccessor.Object);
    }

    static BodyWeight GetValidBodyWeight()
    {
        var validBodyWeight = new BodyWeight()
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            Weight = 70,
            WeightType = WeightType.Kilogram,
        };

        return validBodyWeight;
    }

    static IEnumerable<BodyWeight> GetValidBodyWeights()
    {
        var validBodyWeights = new[]{
            new BodyWeight()
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(-150)),
                Weight = 60,
                WeightType = WeightType.Kilogram,
            },
            new BodyWeight()
            {
                Date = DateOnly.FromDateTime(DateTime.Now),
                Weight = 70,
                WeightType = WeightType.Kilogram,
            }
        };

        return validBodyWeights;
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightsAsync_ShouldReturnPagedResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var bodyWeights = GetValidBodyWeights();
        var user = await GetDefaultUserAsync(db);

        SetupMockHttpContextAccessor(user.Id);

        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightsAsync();

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<BodyWeight>>>(result);
        Assert.NotNull(okResult);
        Assert.NotNull(okResult.Value);
        Assert.Equal(bodyWeights.Count(), okResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightsAsync_ShouldReturnFilteredResult_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeights = GetValidBodyWeights();
        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightsAsync(pageSize: 2);

        // Assert
        var okResult = Assert.IsType<ActionResult<ApiResult<BodyWeight>>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(2, okResult.Value.TotalCount);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightsAsync_ShouldReturnBadRequest_WhenInvalidPageSizeOrIndex()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightsController = GetBodyWeightsController(db);

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightsAsync(pageIndex: -1, pageSize: 0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid page index or page size.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightsAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.GetUserBodyWeightsAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<IQueryable<BodyWeight>>.Fail("Failed to get user body weights."));

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightsAsync();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user body weights.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightsAsync_ShouldReturnBadRequest_WhenBodyWeightsNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.GetUserBodyWeightsAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<IQueryable<BodyWeight>>.Ok(null));

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightsAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Body weights not found.", badRequestResult.Value);
    }


    [Fact]
    public async Task GetCurrentUserBodyWeightByIdAsync_ShouldReturnBadRequest_WhenInvalidId()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightsController = GetBodyWeightsController(db);

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightByIdAsync(0);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Invalid BodyWeight ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightByIdAsync_ShouldReturnBodyWeightById_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightByIdAsync(bodyWeight.Id);

        // Assert
        var okResult = Assert.IsType<ActionResult<BodyWeight>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(bodyWeight, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightByIdAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.GetUserBodyWeightByIdAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult<BodyWeight>.Fail("Failed to get user body weight by ID."));

        var defaultID = 1;

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightByIdAsync(defaultID);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user body weight by ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightByIdAsync_ShouldReturnBadRequest_WhenBodyWeightNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundID = 1;

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightByIdAsync(notFoundID);

        // Assert
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Body weight not found.", notFoundObjectResult.Value);
    }



    [Fact]
    public async Task GetCurrentUserBodyWeightByDateAsync_ShouldReturnBodyWeightByDate_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightByDateAsync(bodyWeight.Date.ToDateTime(new TimeOnly()));

        // Assert
        var okResult = Assert.IsType<ActionResult<BodyWeight>>(result);
        Assert.NotNull(okResult.Value);
        Assert.Equal(bodyWeight, okResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightByDateAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.GetUserBodyWeightByDateAsync(It.IsAny<string>(), It.IsAny<DateOnly>()))
            .ReturnsAsync(ServiceResult<BodyWeight>.Fail("Failed to get user body weight by date."));

        var defaultDateTime = DateTime.Now;

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightByDateAsync(defaultDateTime);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user body weight by date.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetCurrentUserBodyWeightByDateAsync_ShouldReturnBadRequest_WhenBodyWeightNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var notFoundDateTime = DateTime.Now.AddDays(-1111);

        // Act
        var result = await bodyWeightsController.GetCurrentUserBodyWeightByDateAsync(notFoundDateTime);

        // Assert
        var notFoundObjectResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Body weight not found.", notFoundObjectResult.Value);
    }



    [Fact]
    public async Task GetMinCurrentUserBodyWeightAsync_ShouldReturnBodyWeight_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeights = GetValidBodyWeights();
        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        // Act
        var result = await bodyWeightsController.GetMinCurrentUserBodyWeightAsync();

        // Assert
        var okResult = Assert.IsType<ActionResult<BodyWeight>>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMinCurrentUserBodyWeightAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.GetMinUserBodyWeightAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<BodyWeight>.Fail("Failed to get user min body weight."));

        // Act
        var result = await bodyWeightsController.GetMinCurrentUserBodyWeightAsync();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user min body weight.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMinCurrentUserBodyWeightAsync_ShouldReturnBadRequest_WhenBodyWeightNotFound()
    {
        //Arrange
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.GetMinUserBodyWeightAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<BodyWeight>.Ok(null));

        // Act
        var result = await bodyWeightsController.GetMinCurrentUserBodyWeightAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Body weight not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task GetMaxCurrentUserBodyWeightAsync_ShouldReturnBodyWeight_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeights = GetValidBodyWeights();
        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        // Act
        var result = await bodyWeightsController.GetMaxCurrentUserBodyWeightAsync();

        // Assert
        var okResult = Assert.IsType<ActionResult<BodyWeight>>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetMaxCurrentUserBodyWeightAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.GetMaxUserBodyWeightAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<BodyWeight>.Fail("Failed to get user max body weight."));

        // Act
        var result = await bodyWeightsController.GetMaxCurrentUserBodyWeightAsync();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to get user max body weight.", badRequestResult.Value);
    }

    [Fact]
    public async Task GetMaxCurrentUserBodyWeightAsync_ShouldReturnBadRequest_WhenBodyWeightNotFound()
    {
        //Arrange
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.GetMaxUserBodyWeightAsync(It.IsAny<string>()))
            .ReturnsAsync(ServiceResult<BodyWeight>.Ok(null));

        // Act
        var result = await bodyWeightsController.GetMaxCurrentUserBodyWeightAsync();

        // Assert
        var badRequestResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Body weight not found.", badRequestResult.Value);
    }



    [Fact]
    public async Task AddBodyWeightToCurrentUserAsync_ShouldCreateBodyWeight_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeight = GetValidBodyWeight();

        // Act
        var result = await bodyWeightsController.AddBodyWeightToCurrentUserAsync(bodyWeight);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(BodyWeightsController.GetCurrentUserBodyWeightByIdAsync), createdAtActionResult.ActionName);
        Assert.NotNull(createdAtActionResult.Value);
        Assert.Equal(bodyWeight, createdAtActionResult.Value);
    }

    [Fact]
    public async Task AddBodyWeightToCurrentUserAsync_ShouldReturnBadRequest_WhenBodyWeightIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightsController = GetBodyWeightsController(db);

        // Act
        var result = await bodyWeightsController.AddBodyWeightToCurrentUserAsync(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Body weight entry is null.", badRequestResult.Value); 
    }

    [Fact]
    public async Task AddBodyWeightToCurrentUserAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightsController = GetBodyWeightsController(db);

        var bodyWeight = GetValidBodyWeight();
        bodyWeight.Id = 1; //Invalid ID

        // Act
        var result = await bodyWeightsController.AddBodyWeightToCurrentUserAsync(bodyWeight);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("BodyWeight ID must not be set when adding a new body weight.", badRequestResult.Value); 
    }

    [Fact]
    public async Task AddBodyWeightToCurrentUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.AddBodyWeightToUserAsync(It.IsAny<string>(), It.IsAny<BodyWeight>()))
            .ReturnsAsync(ServiceResult<BodyWeight>.Fail("Failed to add body weight to user."));

        var bodyWeight = GetValidBodyWeight();

        // Act
        var result = await bodyWeightsController.AddBodyWeightToCurrentUserAsync(bodyWeight);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        Assert.Equal("Failed to add body weight to user.", badRequestResult.Value); 
    }


    [Fact]
    public async Task UpdateCurrentUserBodyWeightAsync_ShouldUpdateBodyWeight_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        bodyWeight.Weight = 78;

        // Act
        var result = await bodyWeightsController.UpdateCurrentUserBodyWeightAsync(bodyWeight.Id, bodyWeight);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task UpdateCurrentUserBodyWeightAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        bodyWeight.Id = -1; //Invalid ID
        bodyWeight.Weight = 78;

        // Act
        var result = await bodyWeightsController.UpdateCurrentUserBodyWeightAsync(bodyWeight.Id, bodyWeight);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid BodyWeight ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserBodyWeightAsync_ShouldReturnBadRequest_WhenBodyWeightIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        // Act
        var result = await bodyWeightsController.UpdateCurrentUserBodyWeightAsync(bodyWeight.Id, null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Body weight entry is null.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserBodyWeightAsync_ShouldReturnBadRequest_WhenIDsDoNotMatch()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        // Act
        var result = await bodyWeightsController.UpdateCurrentUserBodyWeightAsync(bodyWeight.Id + 1, bodyWeight);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("BodyWeight IDs do not match.", badRequestResult.Value);
    }

    [Fact]
    public async Task UpdateCurrentUserBodyWeightAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.UpdateUserBodyWeightAsync(It.IsAny<string>(), It.IsAny<BodyWeight>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to update user body weight."));

        var bodyWeight = GetValidBodyWeight();
        bodyWeight.Id = 1;

        // Act
        var result = await bodyWeightsController.UpdateCurrentUserBodyWeightAsync(bodyWeight.Id, bodyWeight);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to update user body weight.", badRequestResult.Value);
    }



    [Fact]
    public async Task DeleteBodyWeightFromCurrentUserAsync_ShouldDeleteBodyWeight_WhenValidRequest()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);
        var bodyWeightsController = GetBodyWeightsController(db);

        var user = await GetDefaultUserAsync(db);
        SetupMockHttpContextAccessor(user.Id);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        // Act
        var result = await bodyWeightsController.DeleteBodyWeightFromCurrentUserAsync(bodyWeight.Id);

        // Assert
        Assert.IsAssignableFrom<ActionResult>(result);
    }

    [Fact]
    public async Task DeleteBodyWeightFromCurrentUserAsync_ShouldReturnBadRequest_WhenInvalidID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightsController = GetBodyWeightsController(db);

        long invalidBodyWeightId = -1;

        // Act
        var result = await bodyWeightsController.DeleteBodyWeightFromCurrentUserAsync(invalidBodyWeightId);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid BodyWeight ID.", badRequestResult.Value);
    }

    [Fact]
    public async Task DeleteBodyWeightFromCurrentUserAsync_ShouldReturnBadRequest_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var mockBodyWeightService = new Mock<IBodyWeightService>();
        var bodyWeightsController = new BodyWeightsController(mockBodyWeightService.Object, mockHttpContextAccessor.Object);

        mockBodyWeightService
            .Setup(x => x.DeleteBodyWeightFromUserAsync(It.IsAny<string>(), It.IsAny<long>()))
            .ReturnsAsync(ServiceResult.Fail("Failed to delete user body weight."));

        var bodyWeight = GetValidBodyWeight();
        bodyWeight.Id = 1;

        // Act
        var result = await bodyWeightsController.DeleteBodyWeightFromCurrentUserAsync(bodyWeight.Id);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Failed to delete user body weight.", badRequestResult.Value);
    }
}
