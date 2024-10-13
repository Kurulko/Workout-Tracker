using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Providers;
using WorkoutTrackerAPI.Repositories;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Repositories.WorkoutRepositories;

public class ExerciseRepository_Tests : BaseWorkoutRepository_Tests<Exercise>
{
    async Task<Exercise> CreateExerciseAsync(WorkoutDbContext db, string name, ExerciseType exerciseType, params string[] muscleNames)
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

    async Task<Exercise> GetValidExerciseAsync(WorkoutDbContext db)
    {
        return await CreateExerciseAsync(db, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
    }

    async Task<IEnumerable<Exercise>> GetValidExercisesAsync(WorkoutDbContext db)
    {
        var plankExercise = await CreateExerciseAsync(db, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
        var pullUpExercise = await CreateExerciseAsync(db, "Pull Up", ExerciseType.Reps, "Latissimus dorsi", "Biceps brachii", "Teres minor");
        var pushUpExercise = await CreateExerciseAsync(db, "Push Up", ExerciseType.Reps, "Pectoralis major", "Triceps brachii", "Deltoids");

        var validExercises = new[] { plankExercise, pullUpExercise, pushUpExercise };

        return validExercises;
    }

    async Task<Exercise> GetInvalidExerciseAsync(WorkoutDbContext db)
    {
        var invalidExercise =  await CreateExerciseAsync(db, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");

        invalidExercise.Id = -1;

        return invalidExercise;
    }

    async Task<IEnumerable<Exercise>> GetInvalidExercisesAsync(WorkoutDbContext db)
    {
        var plankExercise = await CreateExerciseAsync(db, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
        var pullUpExercise = await CreateExerciseAsync(db, "Pull Up", ExerciseType.Reps, "Latissimus dorsi", "Biceps brachii", "Teres minor");
        var pushUpExercise = await CreateExerciseAsync(db, "Push Up", ExerciseType.Reps, "Pectoralis major", "Triceps brachii", "Deltoids");

        plankExercise.Id = -1;

        var invalidExercises = new[] { plankExercise, pullUpExercise, pushUpExercise };
        return invalidExercises;
    }

    [Fact]
    public async Task AddAsync_ShouldReturnNewExercise()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);

        //Act & Assert
        await AddModel_ShouldReturnNewModel(exerciseRepository, validExercise);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var invalidExercise = await GetInvalidExerciseAsync(db);

        //Act & Assert
        await AddModel_ShouldThrowException(exerciseRepository, invalidExercise);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddExercisesSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercises = await GetValidExercisesAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldAddModelsSuccessfully(exerciseRepository, validExercises);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var invalidExercises = await GetInvalidExercisesAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldThrowException(exerciseRepository, invalidExercises);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveExerciseSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);
        await exerciseRepository.AddAsync(validExercise);

        //Act & Assert
        await RemoveModel_ShouldRemoveModelSuccessfully(exerciseRepository, validExercise.Id);
    }

    [Fact]
    public async Task RemoveAsync_IncorrectExerciseID_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        //Act & Assert
        await RemoveModel_IncorrectModelID_ShouldThrowException(exerciseRepository);
    }

    [Fact]
    public async Task RemoveAsync_ExerciseNotExist_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        //Act & Assert
        await RemoveModels_ModelNotExist_ShouldThrowException(exerciseRepository, 1);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldRemoveExercisesSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercises = await GetValidExercisesAsync(db);
        await exerciseRepository.AddRangeAsync(validExercises);

        //Act & Assert
        await RemoveRangeModels_ShouldRemoveModelsSuccessfully(exerciseRepository, validExercises);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var invalidExercises = await GetInvalidExercisesAsync(db);

        //Act & Assert
        await RemoveRangeModels_ShouldThrowException(exerciseRepository, invalidExercises);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnExercises()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercises = await GetValidExercisesAsync(db);
        await exerciseRepository.AddRangeAsync(validExercises);

        //Act & Assert
        await GetAllModels_ShouldReturnModels(exerciseRepository, validExercises.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        //Act & Assert
        await GetAllModels_ShouldReturnEmpty(exerciseRepository);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnExerciseById()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);
        await exerciseRepository.AddAsync(validExercise);

        //Act & Assert
        await GetModelById_ShouldReturnModelById(exerciseRepository, validExercise.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        //Act & Assert
        await GetModelById_ShouldReturnNull(exerciseRepository, 1);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnExerciseByName()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);
        await exerciseRepository.AddAsync(validExercise);

        //Act & Assert
        await GetModelByName_ShouldReturnModelByName(exerciseRepository, validExercise.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        //Act & Assert
        await GetModelByName_ShouldReturnNull(exerciseRepository, "Non-existent exercise");
    }

    [Fact]
    public async Task FindAsync_ShouldFindExercises()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var plankExercise = await CreateExerciseAsync(db, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
        var pullUpExercise = await CreateExerciseAsync(db, "Pull Up", ExerciseType.Reps, "Latissimus dorsi", "Biceps brachii", "Teres minor");
        var pushUpExercise = await CreateExerciseAsync(db, "Push Up", ExerciseType.Reps, "Pectoralis major", "Triceps brachii", "Deltoids");

        var validExercises = new[] { plankExercise, pullUpExercise, pushUpExercise };

        await exerciseRepository.AddRangeAsync(validExercises);

        //Act & Assert
        await FindModels_ShouldFindModels(exerciseRepository, m => m.Name.StartsWith("Pu") && m.Name.EndsWith("Up"), 2);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var plankExercise = await CreateExerciseAsync(db, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
        var pullUpExercise = await CreateExerciseAsync(db, "Pull Up", ExerciseType.Reps, "Latissimus dorsi", "Biceps brachii", "Teres minor");
        var pushUpExercise = await CreateExerciseAsync(db, "Push Up", ExerciseType.Reps, "Pectoralis major", "Triceps brachii", "Deltoids");

        var validExercises = new[] { plankExercise, pullUpExercise, pushUpExercise };

        await exerciseRepository.AddRangeAsync(validExercises);

        //Act & Assert
        await FindModels_ShouldReturnEmpty(exerciseRepository, m => m.Type == ExerciseType.WeightAndReps);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);
        await exerciseRepository.AddAsync(validExercise);

        //Act & Assert
        await ModelExists_ShouldReturnTrue(exerciseRepository, validExercise.Id);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        //Act & Assert
        await ModelExists_ShouldReturnFalse(exerciseRepository, 1);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);
        await exerciseRepository.AddAsync(validExercise);

        //Act & Assert
        await ModelExistsByName_ShouldReturnTrue(exerciseRepository, validExercise.Name);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        //Act & Assert
        await ModelExistsByName_ShouldReturnFalse(exerciseRepository, "Non-existent exercise");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);
        await exerciseRepository.AddAsync(validExercise);

        validExercise.Name = "New Name";

        //Act & Assert
        await UpdateModel_ShouldUpdateSuccessfully(exerciseRepository, validExercise);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);
        await exerciseRepository.AddAsync(validExercise);

        validExercise.Id = -1;

        //Act & Assert
        await UpdateModel_ShouldThrowException(exerciseRepository, validExercise);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesEntitySuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);
        await exerciseRepository.AddAsync(validExercise);

        validExercise.Name = "New Name";

        //Act & Assert
        await SaveModelChanges_SaveEntitySuccessfully(exerciseRepository, validExercise);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var exerciseRepository = new ExerciseRepository(db);

        var validExercise = await GetValidExerciseAsync(db);
        await exerciseRepository.AddAsync(validExercise);

        validExercise.Id = -1;

        //Act & Assert
        await SaveModelChanges_ShouldThrowException(exerciseRepository);
    }
}