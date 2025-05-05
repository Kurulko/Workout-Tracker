using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Repositories;
using Xunit;
using WorkoutTracker.API.Initializers;

namespace WorkoutTracker.API.Tests.Repositories.WorkoutRepositories;

public class WorkoutRepository_Tests : BaseWorkoutRepository_Tests<Workout>
{
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

    async Task<Workout> GetInvalidWorkoutAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var plankExercise = await GetPlankExercise(db);
        var legRaiseExercise = await GetLegRaiseExercise(db);

        var invalidWorkout = new Workout()
        {
            Name = "Abs and legs",
            Exercises = new[] { plankExercise, legRaiseExercise },
            UserId = user.Id
        };

        invalidWorkout.Id = -1;

        return invalidWorkout;
    }

    async Task<IEnumerable<Workout>> GetInvalidWorkoutsAsync(WorkoutDbContext db)
    {
        User user = await GetDefaultUserAsync(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var plankExercise = await GetPlankExercise(db);
        var legRaiseExercise = await GetLegRaiseExercise(db);
        var standingCalfRaisesExercise = await GetStandingCalfRaisesExercise(db);

        var validWorkouts = new[] {
            new Workout()
            {
                Id = -1,
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
    public async Task AddAsync_ShouldReturnNewWorkout()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);

        //Act & Assert
        await AddModel_ShouldReturnNewModel(workoutRepository, validWorkout);
    }

    [Fact]
    public async Task AddAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var invalidWorkout = await GetInvalidWorkoutAsync(db);

        //Act & Assert
        await AddModel_ShouldThrowException(workoutRepository, invalidWorkout);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldAddWorkoutsSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkouts = await GetValidWorkoutsAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldAddModelsSuccessfully(workoutRepository, validWorkouts);
    }

    [Fact]
    public async Task AddRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var invalidWorkouts = await GetInvalidWorkoutsAsync(db);

        //Act & Assert
        await AddRangeModels_ShouldThrowException(workoutRepository, invalidWorkouts);
    }

    [Fact]
    public async Task RemoveAsync_ShouldRemoveWorkoutSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        await workoutRepository.AddAsync(validWorkout);

        //Act & Assert
        await RemoveModel_ShouldRemoveModelSuccessfully(workoutRepository, validWorkout.Id);
    }

    [Fact]
    public async Task RemoveAsync_IncorrectWorkoutID_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        //Act & Assert
        await RemoveModel_IncorrectModelID_ShouldThrowException(workoutRepository);
    }

    [Fact]
    public async Task RemoveAsync_WorkoutNotExist_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        //Act & Assert
        await RemoveModels_ModelNotExist_ShouldThrowException(workoutRepository, 1);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldRemoveWorkoutsSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkouts = await GetValidWorkoutsAsync(db);
        await workoutRepository.AddRangeAsync(validWorkouts);

        //Act & Assert
        await RemoveRangeModels_ShouldRemoveModelsSuccessfully(workoutRepository, validWorkouts);
    }

    [Fact]
    public async Task RemoveRangeAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        var invalidWorkouts = await GetInvalidWorkoutsAsync(db);

        //Act & Assert
        await RemoveRangeModels_ShouldThrowException(workoutRepository, invalidWorkouts);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnWorkouts()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkouts = await GetValidWorkoutsAsync(db);
        await workoutRepository.AddRangeAsync(validWorkouts);

        //Act & Assert
        await GetAllModels_ShouldReturnModels(workoutRepository, validWorkouts.Count());
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        //Act & Assert
        await GetAllModels_ShouldReturnEmpty(workoutRepository);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnWorkoutById()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        await workoutRepository.AddAsync(validWorkout);

        //Act & Assert
        await GetModelById_ShouldReturnModelById(workoutRepository, validWorkout.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        //Act & Assert
        await GetModelById_ShouldReturnNull(workoutRepository, 1);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnWorkoutByName()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        await workoutRepository.AddAsync(validWorkout);

        //Act & Assert
        await GetModelByName_ShouldReturnModelByName(workoutRepository, validWorkout.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ShouldReturnNull()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        //Act & Assert
        await GetModelByName_ShouldReturnNull(workoutRepository, "Non-existent workout");
    }

    [Fact]
    public async Task FindAsync_ShouldFindWorkouts()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        User user = await GetDefaultUserAsync(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var plankExercise = await GetPlankExercise(db);
        var legRaiseExercise = await GetLegRaiseExercise(db);
        var standingCalfRaisesExercise = await GetStandingCalfRaisesExercise(db);

        var validWorkouts = new[] {
            new Workout()
            {
                Name = "Abs",
                Exercises = new[] { plankExercise },
                UserId = user.Id
            },
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

        await workoutRepository.AddRangeAsync(validWorkouts);

        //Act & Assert
        await FindModels_ShouldFindModels(workoutRepository, m => m.Name.StartsWith("Abs"), 2);
    }

    [Fact]
    public async Task FindAsync_ShouldReturnEmpty()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        User user = await GetDefaultUserAsync(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var plankExercise = await GetPlankExercise(db);
        var legRaiseExercise = await GetLegRaiseExercise(db);
        var standingCalfRaisesExercise = await GetStandingCalfRaisesExercise(db);

        var validWorkouts = new[] {
            new Workout()
            {
                Name = "Abs",
                Exercises = new[] { plankExercise },
                UserId = user.Id
            },
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

        await workoutRepository.AddRangeAsync(validWorkouts);

        //Act & Assert
        await FindModels_ShouldReturnEmpty(workoutRepository, m => m.Name.StartsWith("Biceps"));
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        await workoutRepository.AddAsync(validWorkout);

        //Act & Assert
        await ModelExists_ShouldReturnTrue(workoutRepository, validWorkout.Id);
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        //Act & Assert
        await ModelExists_ShouldReturnFalse(workoutRepository, 1);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        await workoutRepository.AddAsync(validWorkout);

        //Act & Assert
        await ModelExistsByName_ShouldReturnTrue(workoutRepository, validWorkout.Name);
    }

    [Fact]
    public async Task ExistsByNameAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        //Act & Assert
        await ModelExistsByName_ShouldReturnFalse(workoutRepository, "Non-existent workout");
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        await workoutRepository.AddAsync(validWorkout);

        validWorkout.Name = "New Name";

        //Act & Assert
        await UpdateModel_ShouldUpdateSuccessfully(workoutRepository, validWorkout);
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        await workoutRepository.AddAsync(validWorkout);

        validWorkout.Id = -1;

        //Act & Assert
        await UpdateModel_ShouldThrowException(workoutRepository, validWorkout);
    }

    [Fact]
    public async Task SaveChangesAsync_SavesEntitySuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        await workoutRepository.AddAsync(validWorkout);

        validWorkout.Name = "New Name";

        //Act & Assert
        await SaveModelChanges_SaveEntitySuccessfully(workoutRepository, validWorkout);
    }

    [Fact]
    public async Task SaveChangesAsync_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var workoutRepository = new WorkoutRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var validWorkout = await GetValidWorkoutAsync(db);
        await workoutRepository.AddAsync(validWorkout);

        validWorkout.Id = -1;

        //Act & Assert
        await SaveModelChanges_ShouldThrowException(workoutRepository);
    }
}
