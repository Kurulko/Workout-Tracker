using Microsoft.EntityFrameworkCore.Internal;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services;
using Xunit;
using WorkoutTrackerAPI.Services.BodyWeightServices;
using FakeItEasy;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Moq;
using System.Linq.Expressions;

namespace WorkoutTrackerAPI.Tests.Services.UserServices;

public class BodyWeightService_Tests : DbModelService_Tests<BodyWeight>
{
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

    static IBodyWeightService GetBodyWeightService(WorkoutDbContext db)
    {
        var bodyWeightRepository = new BodyWeightRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new BodyWeightService(bodyWeightRepository, userRepository);
    }

    [Fact]
    public async Task AddBodyWeightToUser_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var validBodyWeight = GetValidBodyWeight();
        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await bodyWeightService.AddBodyWeightToUserAsync(user.Id, validBodyWeight);

        // Assert
        Assert.True(result.Success);
        Assert.NotEqual(0, validBodyWeight.Id);
        Assert.Equal(validBodyWeight, result.Model);
    }

    [Fact]
    public async Task AddBodyWeightToUser_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var validBodyWeight = GetValidBodyWeight();

        // Act
        var result = await bodyWeightService.AddBodyWeightToUserAsync(null!, validBodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddBodyWeightToUser_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var validBodyWeight = GetValidBodyWeight();
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await bodyWeightService.AddBodyWeightToUserAsync(nonExistenceUserId, validBodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddBodyWeightToUser_ShouldReturnFail_WhenBodyWeightIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await bodyWeightService.AddBodyWeightToUserAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Body weight entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddBodyWeightToUser_ShouldReturnFail_WhenBodyWeightIdIsNonZero()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);
        var validBodyWeight = GetValidBodyWeight();
        validBodyWeight.Id = 1;

        // Act
        var result = await bodyWeightService.AddBodyWeightToUserAsync(user.Id, validBodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("BodyWeight ID must not be set when adding a new body weight.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddBodyWeightToUser_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var validBodyWeight = GetValidBodyWeight();

        var user = await GetDefaultUserAsync(db);

        bodyWeightRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<BodyWeight>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await bodyWeightService.AddBodyWeightToUserAsync(user.Id, validBodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to add body weight", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteBodyWeightFromUser_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var bodyWeight = GetValidBodyWeight();
        var user = await GetDefaultUserAsync(db);
        string userId = user.Id;

        await bodyWeightService.AddBodyWeightToUserAsync(userId, bodyWeight);

        // Act
        var result = await bodyWeightService.DeleteBodyWeightFromUserAsync(userId, bodyWeight.Id);

        // Assert
        Assert.True(result.Success);

        var resultById = await bodyWeightService.GetUserBodyWeightByIdAsync(userId, bodyWeight.Id);
        Assert.Null(resultById.Model);
    }

    [Fact]
    public async Task DeleteBodyWeightFromUser_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var bodyWeight = GetValidBodyWeight();

        // Act
        var result = await bodyWeightService.DeleteBodyWeightFromUserAsync(null!, bodyWeight.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteBodyWeightFromUser_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var bodyWeight = GetValidBodyWeight();
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await bodyWeightService.DeleteBodyWeightFromUserAsync(nonExistenceUserId, bodyWeight.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteBodyWeightFromUser_ShouldReturnFail_WhenInvalidBodyWeightID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);
        long invalidBodyWeightID = -1;

        // Act
        var result = await bodyWeightService.DeleteBodyWeightFromUserAsync(user.Id, invalidBodyWeightID);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid BodyWeight ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteBodyWeightFromUser_ShouldReturnFail_WhenBodyWeightNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        long nonExistenceBodyWeightId = 100;

        // Act
        var result = await bodyWeightService.DeleteBodyWeightFromUserAsync(user.Id, nonExistenceBodyWeightId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Body weight not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteBodyWeightFromUser_ShouldReturnFail_AccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();

        bodyWeight.Id = 1;

        bodyWeightRepositoryMock
            .Setup(repo => repo.GetByIdAsync(bodyWeight.Id))
            .ReturnsAsync(() =>
            {
                bodyWeight.UserId = Guid.NewGuid().ToString();
                return bodyWeight;
            });

        // Act
        var result = await bodyWeightService.DeleteBodyWeightFromUserAsync(user.Id, bodyWeight.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User does not have permission to delete this body weight entry", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteBodyWeightFromUser_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();

        bodyWeight.Id = 1;
        bodyWeight.UserId = user.Id;

        bodyWeightRepositoryMock
            .Setup(repo => repo.GetByIdAsync(bodyWeight.Id))
            .ReturnsAsync(() => bodyWeight);

        bodyWeightRepositoryMock
            .Setup(repo => repo.RemoveAsync(bodyWeight.Id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await bodyWeightService.DeleteBodyWeightFromUserAsync(user.Id, bodyWeight.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to delete body weight", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserBodyWeights_ShouldReturnBodyWeights_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var bodyWeights = GetValidBodyWeights();
        var user = await GetDefaultUserAsync(db);

        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        // Act
        var result = await bodyWeightService.GetUserBodyWeightsAsync(user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(bodyWeights.Count(), result.Model!.Count());
    }

    [Fact]
    public async Task GetUserBodyWeights_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightsAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserBodyWeights_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await bodyWeightService.GetUserBodyWeightsAsync(nonExistenceUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserBodyWeights_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();

        bodyWeightRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<BodyWeight, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await bodyWeightService.GetUserBodyWeightsAsync(user.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get body weights", result.ErrorMessage);
    }


    [Fact]
    public async Task GetMaxUserBodyWeight_ShouldReturnMaxBodyWeight_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeights = GetValidBodyWeights();
        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        // Act
        var result = await bodyWeightService.GetMaxUserBodyWeightAsync(user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);

        var maxBodyWeight = bodyWeights.MaxBy(bw => BodyWeight.GetBodyWeightInKilos(bw));
        Assert.Equal(maxBodyWeight, result.Model);
    }

    [Fact]
    public async Task GetMaxUserBodyWeight_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        // Act
        var result = await bodyWeightService.GetMaxUserBodyWeightAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMaxUserBodyWeight_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await bodyWeightService.GetMaxUserBodyWeightAsync(nonExistenceUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMaxUserBodyWeight_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();

        bodyWeightRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<BodyWeight, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await bodyWeightService.GetMaxUserBodyWeightAsync(user.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get max body weight", result.ErrorMessage);
    }


    [Fact]
    public async Task GetMinUserBodyWeight_ShouldReturnMaxBodyWeight_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeights = GetValidBodyWeights();
        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        // Act
        var result = await bodyWeightService.GetMinUserBodyWeightAsync(user.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);

        var maxBodyWeight = bodyWeights.MinBy(bw => BodyWeight.GetBodyWeightInKilos(bw));
        Assert.Equal(maxBodyWeight, result.Model);
    }

    [Fact]
    public async Task GetMinUserBodyWeight_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        // Act
        var result = await bodyWeightService.GetMinUserBodyWeightAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMinUserBodyWeight_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await bodyWeightService.GetMinUserBodyWeightAsync(nonExistenceUserId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMinUserBodyWeight_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();

        bodyWeightRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<BodyWeight, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await bodyWeightService.GetMinUserBodyWeightAsync(user.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get min body weight", result.ErrorMessage);
    }


    [Fact]
    public async Task GetUserBodyWeightByDate_ShouldReturnBodyWeightByDate_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeights = GetValidBodyWeights();
        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByDateAsync(user.Id, today);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(today, result.Model.Date);
    }

    [Fact]
    public async Task GetUserBodyWeightByDate_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeights = GetValidBodyWeights();
        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        DateOnly yesterday = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByDateAsync(user.Id, yesterday);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserBodyWeightByDate_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByDateAsync(null!, today);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserBodyWeightByDate_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByDateAsync(nonExistenceUserId, today);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserBodyWeightByDate_ShouldReturnFail_WhenInvalidDate()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeights = GetValidBodyWeights();
        foreach (var bodyWeight in bodyWeights)
        {
            await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);
        }

        DateOnly tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByDateAsync(user.Id, tomorrow);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Incorrect date.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserBodyWeightByDate_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();

        bodyWeightRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<BodyWeight, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByDateAsync(user.Id, today);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get body weight by date", result.ErrorMessage);
    }


    [Fact]
    public async Task GetUserBodyWeightById_ShouldReturnBodyWeightById_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByIdAsync(user.Id, bodyWeight.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(bodyWeight.Id, result.Model.Id);
    }

    [Fact]
    public async Task GetUserBodyWeightById_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByIdAsync(user.Id, 1);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserBodyWeightById_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByIdAsync(null!, bodyWeight.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserBodyWeightById_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByIdAsync(nonExistenceUserId, bodyWeight.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserBodyWeightById_ShouldReturnFail_WhenInvalidBodyWeightID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByIdAsync(user.Id, -1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid BodyWeight ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserBodyWeightById_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();
        bodyWeight.Id = 1;

        bodyWeightRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await bodyWeightService.GetUserBodyWeightByIdAsync(user.Id, bodyWeight.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get body weight", result.ErrorMessage);
    }



    [Fact]
    public async Task UpdateUserBodyWeight_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        bodyWeight.Weight = 115;

        // Act
        var result = await bodyWeightService.UpdateUserBodyWeightAsync(user.Id, bodyWeight);

        // Assert
        Assert.True(result.Success);

        var resultById = await bodyWeightService.GetUserBodyWeightByIdAsync(user.Id, bodyWeight.Id);
        Assert.NotNull(resultById.Model);
        Assert.Equal(bodyWeight.Weight, resultById.Model!.Weight);
    }

    [Fact]
    public async Task UpdateUserBodyWeight_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        bodyWeight.Weight = 115;

        // Act
        var result = await bodyWeightService.UpdateUserBodyWeightAsync(null!, bodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserBodyWeight_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        bodyWeight.Weight = 115;

        // Act
        var result = await bodyWeightService.UpdateUserBodyWeightAsync(nonExistenceUserId, bodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserBodyWeight_ShouldReturnFail_WhenBodyWeightIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await bodyWeightService.UpdateUserBodyWeightAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Body weight entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserBodyWeight_ShouldReturnFail_WhenInvalidBodyWeightID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);

        var bodyWeight = GetValidBodyWeight();
        await bodyWeightService.AddBodyWeightToUserAsync(user.Id, bodyWeight);

        bodyWeight.Id = -1;
        bodyWeight.Weight = 115;

        // Act
        var result = await bodyWeightService.UpdateUserBodyWeightAsync(user.Id, bodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid BodyWeight ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserBodyWeight_ShouldReturnFail_WhenBodyWeightNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightService = GetBodyWeightService(db);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();

        bodyWeight.Id = 1;
        bodyWeight.Weight = 115;

        // Act
        var result = await bodyWeightService.UpdateUserBodyWeightAsync(user.Id, bodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Body weight not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserBodyWeight_ShouldReturnFail_WhenAccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();

        bodyWeight.Id = 1;
        bodyWeight.Weight = 115;
        bodyWeight.UserId = Guid.NewGuid().ToString();

        bodyWeightRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => bodyWeight);

        // Act
        var result = await bodyWeightService.UpdateUserBodyWeightAsync(user.Id, bodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User does not have permission to update this body weight entry", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserBodyWeight_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var bodyWeightRepositoryMock = new Mock<BodyWeightRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var bodyWeightService = new BodyWeightService(bodyWeightRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var bodyWeight = GetValidBodyWeight();

        bodyWeight.Id = 1;
        bodyWeight.UserId = user.Id;

        bodyWeightRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => bodyWeight);

        bodyWeightRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<BodyWeight>()))
            .ThrowsAsync(new Exception("Database error"));

        bodyWeight.Weight = 115;

        // Act
        var result = await bodyWeightService.UpdateUserBodyWeightAsync(user.Id, bodyWeight);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to update body weight", result.ErrorMessage);
    }
}