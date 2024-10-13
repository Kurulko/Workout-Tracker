using Moq;
using System.Linq.Expressions;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.MuscleServices;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Services.WorkoutServices;


public class MuscleService_Tests : BaseWorkoutService_Tests<Muscle>
{
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

    static IMuscleService GetMuscleService(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        return new MuscleService(muscleRepository, userRepository);
    }


    [Fact]
    public async Task AddMuscle_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var validMuscle = GetValidMuscle();

        // Act
        var result = await muscleService.AddMuscleAsync(validMuscle);

        // Assert
        Assert.True(result.Success);
        Assert.NotEqual(0, validMuscle.Id);
        Assert.Equal(validMuscle, result.Model);
    }

    [Fact]
    public async Task AddMuscle_ShouldReturnFail_WhenMuscleIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        // Act
        var result = await muscleService.AddMuscleAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Muscle entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddMuscle_ShouldReturnFail_WhenMuscleIdIsNonZero()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var validMuscle = GetValidMuscle();
        validMuscle.Id = 1;

        // Act
        var result = await muscleService.AddMuscleAsync(validMuscle);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Muscle ID must not be set when adding a new muscle.", result.ErrorMessage);
    }

    [Fact]
    public async Task AddMuscle_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepositoryMock = new Mock<MuscleRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleService = new MuscleService(muscleRepositoryMock.Object, userRepository);

        var validMuscle = GetValidMuscle();

        muscleRepositoryMock
            .Setup(repo => repo.AddAsync(It.IsAny<Muscle>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await muscleService.AddMuscleAsync(validMuscle);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to add muscle", result.ErrorMessage);
    }



    [Fact]
    public async Task DeleteMuscle_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscle = GetValidMuscle();

        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await muscleService.DeleteMuscleAsync(muscle.Id);

        // Assert
        Assert.True(result.Success);

        var resultById = await muscleService.GetMuscleByIdAsync(muscle.Id);
        Assert.Null(resultById.Model);
    }

    [Fact]
    public async Task DeleteMuscle_ShouldReturnFail_WhenInvalidMuscleID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        long invalidMuscleID = -1;

        // Act
        var result = await muscleService.DeleteMuscleAsync(invalidMuscleID);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Muscle ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMuscle_ShouldReturnFail_WhenMuscleNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        long nonExistenceMuscleId = 100;

        // Act
        var result = await muscleService.DeleteMuscleAsync(nonExistenceMuscleId);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Muscle not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task DeleteMuscle_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepositoryMock = new Mock<MuscleRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleService = new MuscleService(muscleRepositoryMock.Object, userRepository);

        var muscle = GetValidMuscle();

        muscle.Id = 1;

        muscleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(muscle.Id))
            .ReturnsAsync(() => muscle);

        muscleRepositoryMock
            .Setup(repo => repo.RemoveAsync(muscle.Id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await muscleService.DeleteMuscleAsync(muscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to delete muscle", result.ErrorMessage);
    }


    [Fact]
    public async Task GetMuscles_ShouldReturnMuscles_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscles = GetValidMuscles();

        foreach (var muscle in muscles)
        {
            await muscleService.AddMuscleAsync(muscle);
        }

        // Act
        var result = await muscleService.GetMusclesAsync();

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(muscles.Count(), result.Model.Count());
    }

    [Fact]
    public async Task GetMuscles_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepositoryMock = new Mock<MuscleRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleService = new MuscleService(muscleRepositoryMock.Object, userRepository);

        var muscle = GetValidMuscle();

        muscleRepositoryMock
            .Setup(repo => repo.GetAllAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await muscleService.GetMusclesAsync();

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get muscles", result.ErrorMessage);
    }



    [Fact]
    public async Task GetMuscleByName_ShouldReturnMuscleByName_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);


        var muscles = GetValidMuscles();
        foreach (var muscle in muscles)
        {
            await muscleService.AddMuscleAsync(muscle);
        }

        string muscleName = "Back";

        // Act
        var result = await muscleService.GetMuscleByNameAsync(muscleName);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(muscleName, result.Model.Name);
    }

    [Fact]
    public async Task GetMuscleByName_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscles = GetValidMuscles();
        foreach (var muscle in muscles)
        {
            await muscleService.AddMuscleAsync(muscle);
        }

        string nonExistenceMuscle = "Non-existence muscle";

        // Act
        var result = await muscleService.GetMuscleByNameAsync(nonExistenceMuscle);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetMuscleByName_ShouldReturnFail_WhenInvalidName()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscles = GetValidMuscles();
        foreach (var muscle in muscles)
        {
            await muscleService.AddMuscleAsync(muscle);
        }

        // Act
        var result = await muscleService.GetMuscleByNameAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Muscle name cannot be null or empty.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMuscleByName_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepositoryMock = new Mock<MuscleRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleService = new MuscleService(muscleRepositoryMock.Object, userRepository);

        var muscle = GetValidMuscle();

        muscle.Id = 1;

        muscleRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await muscleService.GetMuscleByNameAsync(muscle.Name);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get muscle by name", result.ErrorMessage);
    }


    [Fact]
    public async Task GetMuscleById_ShouldReturnMuscleById_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await muscleService.GetMuscleByIdAsync(muscle.Id);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Model);
        Assert.Equal(muscle.Id, result.Model.Id);
    }

    [Fact]
    public async Task GetMuscleById_ShouldReturnNull_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        // Act
        var result = await muscleService.GetMuscleByIdAsync(1);

        // Assert
        Assert.True(result.Success);
        Assert.Null(result.Model);
    }

    [Fact]
    public async Task GetMuscleById_ShouldReturnFail_WhenInvalidMuscleID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        // Act
        var result = await muscleService.GetMuscleByIdAsync(-1);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Muscle ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task GetMuscleById_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepositoryMock = new Mock<MuscleRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleService = new MuscleService(muscleRepositoryMock.Object, userRepository);

        var muscle = GetValidMuscle();
        muscle.Id = 1;

        muscleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await muscleService.GetMuscleByIdAsync(muscle.Id);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to get muscle", result.ErrorMessage);
    }


    [Fact]
    public async Task UpdateMuscle_ShouldReturnOk_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);


        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        muscle.Name = "New Name";

        // Act
        var result = await muscleService.UpdateMuscleAsync(muscle);

        // Assert
        Assert.True(result.Success);

        var resultById = await muscleService.GetMuscleByIdAsync(muscle.Id);
        Assert.NotNull(resultById.Model);
        Assert.Equal(muscle.Name, resultById.Model!.Name);
    }


    [Fact]
    public async Task UpdateMuscle_ShouldReturnFail_WhenMuscleIsNull()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        // Act
        var result = await muscleService.UpdateMuscleAsync(null!);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Muscle entry cannot be null.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateMuscle_ShouldReturnFail_WhenInvalidMuscleID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        muscle.Id = -1;
        muscle.Name = "New Name";

        // Act
        var result = await muscleService.UpdateMuscleAsync(muscle);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Invalid Muscle ID.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateMuscle_ShouldReturnFail_WhenMuscleNotFound()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscle = GetValidMuscle();

        muscle.Id = 1;
        muscle.Name = "New Name";

        // Act
        var result = await muscleService.UpdateMuscleAsync(muscle);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Muscle not found.", result.ErrorMessage);
    }

    [Fact]
    public async Task UpdateMuscle_ShouldReturnFail_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepositoryMock = new Mock<MuscleRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleService = new MuscleService(muscleRepositoryMock.Object, userRepository);

        var muscle = GetValidMuscle();

        muscle.Id = 1;

        muscleRepositoryMock
            .Setup(repo => repo.GetByIdAsync(It.IsAny<long>()))
            .ReturnsAsync(() => muscle);

        muscleRepositoryMock
            .Setup(repo => repo.UpdateAsync(It.IsAny<Muscle>()))
            .ThrowsAsync(new Exception("Database error"));

        muscle.Name = "New Name";

        // Act
        var result = await muscleService.UpdateMuscleAsync(muscle);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("Failed to update muscle", result.ErrorMessage);
    }



    [Fact]
    public async Task MuscleExists_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await muscleService.MuscleExistsAsync(muscle.Id);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task MuscleExists_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        // Act
        var result = await muscleService.MuscleExistsAsync(1);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MuscleExists_ShouldThrowException_WhenInvalidMuscleID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        muscle.Id = -1;

        //Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidIDException>(async () => await muscleService.MuscleExistsAsync(muscle.Id));
        Assert.Contains("Invalid Muscle ID.", ex.Message);
    }

    [Fact]
    public async Task MuscleExists_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepositoryMock = new Mock<MuscleRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleService = new MuscleService(muscleRepositoryMock.Object, userRepository);

        var muscle = GetValidMuscle();
        muscle.Id = 1;

        muscleRepositoryMock
            .Setup(repo => repo.ExistsAsync(It.IsAny<long>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await muscleService.MuscleExistsAsync(muscle.Id));
        Assert.Equal("Database error", ex.Message);
    }


    [Fact]
    public async Task MuscleExistsByName_ShouldReturnTrue_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        var muscle = GetValidMuscle();
        await muscleService.AddMuscleAsync(muscle);

        // Act
        var result = await muscleService.MuscleExistsByNameAsync(muscle.Name);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task MuscleExistsByName_ShouldReturnFalse_WhenInputIsValid()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        string nonExistenceName = "Non-existence name";

        // Act
        var result = await muscleService.MuscleExistsByNameAsync(nonExistenceName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task MuscleExistsByName_ShouldThrowException_WhenInvalidMuscleID()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleService = GetMuscleService(db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentNullOrEmptyException>(async () => await muscleService.MuscleExistsByNameAsync(null!));
        Assert.Contains("Muscle name cannot be null or empty.", ex.Message);
    }

    [Fact]
    public async Task MuscleExistsByName_ShouldThrowException_WhenExceptionOccurs()
    {
        // Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepositoryMock = new Mock<MuscleRepository>(db);

        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);
        var muscleService = new MuscleService(muscleRepositoryMock.Object, userRepository);

        var muscle = GetValidMuscle();
        muscle.Id = 1;

        muscleRepositoryMock
            .Setup(repo => repo.ExistsByNameAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("Database error"));

        //Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(async () => await muscleService.MuscleExistsByNameAsync(muscle.Name));
        Assert.Equal("Database error", ex.Message);
    }
}
