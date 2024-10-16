using Moq;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.MuscleSizeServices;
using WorkoutTrackerAPI.Services;
using Xunit;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Initializers;
using System.Linq.Expressions;

namespace WorkoutTrackerAPI.Tests.Services.UserServices;

public class MuscleSizeService_Tests : DbModelService_Tests<MuscleSize>
{
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

    static IMuscleSizeService GetMuscleSizeService(WorkoutDbContext db)
    {
        var muscleSizeRepository = new MuscleSizeRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new MuscleSizeService(muscleSizeRepository, userRepository);
    }

    [Fact]
    public async Task AddMuscleSizeToUser_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, validMuscleSize);

        // Assert
        Assert.True(result.Success);
        Assert.NotEqual(0, validMuscleSize.Id);
        Assert.Equal(validMuscleSize, result.Model);
    }

    [Fact]
    public async Task AddMuscleSizeToUser_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);

        // Act
        var result = await muscleSizeService.AddMuscleSizeToUserAsync(null!, validMuscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddMuscleSizeToUser_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await muscleSizeService.AddMuscleSizeToUserAsync(nonExistenceUserId, validMuscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddMuscleSizeToUser_ShouldReturnFail_WhenMuscleSizeIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Muscle size entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddMuscleSizeToUser_ShouldReturnFail_WhenMuscleSizeIdIsNonZero()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);
        var validMuscleSize = await GetValidMuscleSizeAsync(db);
        validMuscleSize.Id = 1;

        // Act
        var result = await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, validMuscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("MuscleSize ID must not be set when adding a new muscle size.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddMuscleSizeToUser_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var validMuscleSize = await GetValidMuscleSizeAsync(db);

        var user = await GetDefaultUserAsync(db);

        muscleSizeRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<MuscleSize>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, validMuscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to add muscle size", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMuscleSizeFromUser_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        var user = await GetDefaultUserAsync(db);
        string userId = user.Id;

        await muscleSizeService.AddMuscleSizeToUserAsync(userId, muscleSize);

        // Act
        var result = await muscleSizeService.DeleteMuscleSizeFromUserAsync(userId, muscleSize.Id);

        // Assert
        Assert.True(result.Success);

        var resultById = await muscleSizeService.GetUserMuscleSizeByIdAsync(userId, muscleSize.Id);
        Assert.Null(resultById.Model);
    }

    [Fact]
    public async Task DeleteMuscleSizeFromUser_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var muscleSize = await GetValidMuscleSizeAsync(db);

        // Act
        var result = await muscleSizeService.DeleteMuscleSizeFromUserAsync(null!, muscleSize.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMuscleSizeFromUser_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        string nonExistenceUserId = Guid.NewGuid().ToString();

        // Act
        var result = await muscleSizeService.DeleteMuscleSizeFromUserAsync(nonExistenceUserId, muscleSize.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMuscleSizeFromUser_ShouldReturnFail_WhenInvalidMuscleSizeID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);
        long invalidMuscleSizeID = -1;

        // Act
        var result = await muscleSizeService.DeleteMuscleSizeFromUserAsync(user.Id, invalidMuscleSizeID);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid MuscleSize ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMuscleSizeFromUser_ShouldReturnFail_WhenMuscleSizeNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        long nonExistenceMuscleSizeId = 100;

        // Act
        var result = await muscleSizeService.DeleteMuscleSizeFromUserAsync(user.Id, nonExistenceMuscleSizeId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Muscle size not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMuscleSizeFromUser_ShouldReturnFail_AccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);

        muscleSize.Id = 1;

        muscleSizeRepositoryMock
            .Setup(repo => repo.GetByIdAsync(muscleSize.Id))
            .ReturnsAsync(() =>
            {
                muscleSize.UserId = Guid.NewGuid().ToString();
                return muscleSize;
            });

        // Act
        var result = await muscleSizeService.DeleteMuscleSizeFromUserAsync(user.Id, muscleSize.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User does not have permission to delete this muscle size entry", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMuscleSizeFromUser_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);

        muscleSize.Id = 1;
        muscleSize.UserId = user.Id;

        muscleSizeRepositoryMock
            .Setup(repo => repo.GetByIdAsync(muscleSize.Id))
            .ReturnsAsync(() => muscleSize);

        muscleSizeRepositoryMock
            .Setup(repo => repo.RemoveAsync(muscleSize.Id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await muscleSizeService.DeleteMuscleSizeFromUserAsync(user.Id, muscleSize.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to delete muscle size", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserMuscleSizes_ShouldReturnMuscleSizes_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        var user = await GetDefaultUserAsync(db);

        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizesAsync(user.Id, bicepsMuscle.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.All(result.Model, m => Assert.True(m.MuscleId == bicepsMuscle.Id));
    }

    [Fact]
    public async Task GetUserMuscleSizes_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizesAsync(null!, bicepsMuscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserMuscleSizes_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();
        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizesAsync(nonExistenceUserId, bicepsMuscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserMuscleSizes_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);

        muscleSizeRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<MuscleSize, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizesAsync(user.Id, bicepsMuscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get muscle sizes", result.ErrorMessage);
    }


    [Fact]
    public async Task GetMaxUserMuscleSize_ShouldReturnMaxMuscleSize_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetMaxUserMuscleSizeAsync(user.Id, bicepsMuscle.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);

        var maxMuscleSize = muscleSizes.Where(m => m.MuscleId == bicepsMuscle.Id).MaxBy(bw => MuscleSize.GetMuscleSizeInCentimeters(bw));
        Assert.Equal(maxMuscleSize, result.Model);
    }

    [Fact]
    public async Task GetMaxUserMuscleSize_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetMaxUserMuscleSizeAsync(null!, bicepsMuscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMaxUserMuscleSize_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();
        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetMaxUserMuscleSizeAsync(nonExistenceUserId, bicepsMuscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMaxUserMuscleSize_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);

        muscleSizeRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<MuscleSize, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetMaxUserMuscleSizeAsync(user.Id, bicepsMuscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get max muscle size", result.ErrorMessage);
    }


    [Fact]
    public async Task GetMinUserMuscleSize_ShouldReturnMaxMuscleSize_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetMinUserMuscleSizeAsync(user.Id, bicepsMuscle.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);

        var maxMuscleSize = muscleSizes.Where(m => m.MuscleId == bicepsMuscle.Id).MinBy(bw => MuscleSize.GetMuscleSizeInCentimeters(bw));
        Assert.Equal(maxMuscleSize, result.Model);
    }

    [Fact]
    public async Task GetMinUserMuscleSize_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetMinUserMuscleSizeAsync(null!, bicepsMuscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMinUserMuscleSize_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();
        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetMinUserMuscleSizeAsync(nonExistenceUserId, bicepsMuscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMinUserMuscleSize_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);

        muscleSizeRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<MuscleSize, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetMinUserMuscleSizeAsync(user.Id, bicepsMuscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get min muscle size", result.ErrorMessage);
    }


    [Fact]
    public async Task GetUserMuscleSizeByDate_ShouldReturnMuscleSizeByDate_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByDateAsync(user.Id, bicepsMuscle.Id, today);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(today, DateOnly.FromDateTime(result.Model.Date));
    }

    [Fact]
    public async Task GetUserMuscleSizeByDate_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        DateOnly yesterday = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByDateAsync(user.Id, bicepsMuscle.Id, yesterday);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserMuscleSizeByDate_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByDateAsync(null!, bicepsMuscle.Id, today);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserMuscleSizeByDate_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByDateAsync(nonExistenceUserId, bicepsMuscle.Id, today);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserMuscleSizeByDate_ShouldReturnFail_WhenInvalidDate()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSizes = await GetValidMuscleSizesAsync(db);
        foreach (var muscleSize in muscleSizes)
        {
            await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);
        }

        DateOnly tomorrow = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByDateAsync(user.Id, bicepsMuscle.Id, tomorrow);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Incorrect date.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserMuscleSizeByDate_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);

        muscleSizeRepositoryMock
            .Setup(repo => repo.FindAsync(It.IsAny<Expression<Func<MuscleSize, bool>>>()))
            .ThrowsAsync(new Exception("Database error"));

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        var bicepsMuscle = await GetBicepsMuscleAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByDateAsync(user.Id, bicepsMuscle.Id, today);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get muscle size by date", result.ErrorMessage);
    }


    [Fact]
    public async Task GetUserMuscleSizeById_ShouldReturnMuscleSizeById_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByIdAsync(user.Id, muscleSize.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(muscleSize.Id, result.Model.Id);
    }

    [Fact]
    public async Task GetUserMuscleSizeById_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByIdAsync(user.Id, 1);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetUserMuscleSizeById_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByIdAsync(null!, muscleSize.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserMuscleSizeById_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByIdAsync(nonExistenceUserId, muscleSize.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserMuscleSizeById_ShouldReturnFail_WhenInvalidMuscleSizeID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByIdAsync(user.Id, -1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid MuscleSize ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetUserMuscleSizeById_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);
        muscleSize.Id = 1;

        muscleSizeRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        DateOnly today = DateOnly.FromDateTime(DateTime.Now);

        // Act
        var result = await muscleSizeService.GetUserMuscleSizeByIdAsync(user.Id, muscleSize.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get muscle size", result.ErrorMessage);
    }



    [Fact]
    public async Task UpdateUserMuscleSize_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        muscleSize.Size = 42;

        // Act
        var result = await muscleSizeService.UpdateUserMuscleSizeAsync(user.Id, muscleSize);

        // Assert
        Assert.True(result.Success);

        var resultById = await muscleSizeService.GetUserMuscleSizeByIdAsync(user.Id, muscleSize.Id);
        Assert.NotNull(resultById.Model);
        Assert.Equal(muscleSize.Size, resultById.Model!.Size);
    }

    [Fact]
    public async Task UpdateUserMuscleSize_ShouldReturnFail_WhenUserIdIsNullOrEmpty()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        muscleSize.Size = 42;

        // Act
        var result = await muscleSizeService.UpdateUserMuscleSizeAsync(null!, muscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User ID cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserMuscleSize_ShouldReturnFail_WhenUserNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        string nonExistenceUserId = Guid.NewGuid().ToString();

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        muscleSize.Size = 42;

        // Act
        var result = await muscleSizeService.UpdateUserMuscleSizeAsync(nonExistenceUserId, muscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("User not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserMuscleSize_ShouldReturnFail_WhenMuscleSizeIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        // Act
        var result = await muscleSizeService.UpdateUserMuscleSizeAsync(user.Id, null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Muscle size entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserMuscleSize_ShouldReturnFail_WhenInvalidMuscleSizeID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);

        var muscleSize = await GetValidMuscleSizeAsync(db);
        await muscleSizeService.AddMuscleSizeToUserAsync(user.Id, muscleSize);

        muscleSize.Id = -1;
        muscleSize.Size = 42;

        // Act
        var result = await muscleSizeService.UpdateUserMuscleSizeAsync(user.Id, muscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid MuscleSize ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserMuscleSize_ShouldReturnFail_WhenMuscleSizeNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeService = GetMuscleSizeService(db);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);

        muscleSize.Id = 1;
        muscleSize.Size = 42;

        // Act
        var result = await muscleSizeService.UpdateUserMuscleSizeAsync(user.Id, muscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Muscle size not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserMuscleSize_ShouldReturnFail_WhenAccessDenied()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);

        muscleSize.Id = 1;
        muscleSize.Size = 42;
        muscleSize.UserId = Guid.NewGuid().ToString();

        muscleSizeRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => muscleSize);

        // Act
        var result = await muscleSizeService.UpdateUserMuscleSizeAsync(user.Id, muscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("User does not have permission to update this muscle size entry", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateUserMuscleSize_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleSizeRepositoryMock = new Mock<MuscleSizeRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleSizeService = new MuscleSizeService(muscleSizeRepositoryMock.Object, userRepository);

        var user = await GetDefaultUserAsync(db);
        var muscleSize = await GetValidMuscleSizeAsync(db);

        muscleSize.Id = 1;
        muscleSize.UserId = user.Id;

        muscleSizeRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => muscleSize);

        muscleSizeRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<MuscleSize>()))
            .ThrowsAsync(new Exception("Database error"));

        muscleSize.Size = 42;

        // Act
        var result = await muscleSizeService.UpdateUserMuscleSizeAsync(user.Id, muscleSize);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to update muscle size", result.ErrorMessage);
    }
}