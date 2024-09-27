using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.MuscleServices;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.RoleServices;
using Xunit;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Repositories.UserRepositories;

namespace WorkoutTrackerAPI.Tests.Initializers;

public class ExercisesInitializer_Tests
{
    [Fact]
    public static async Task InitializeAsync()
    {
        //Arrange
        WorkoutContextFactory factory = new();
        using var db = factory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var exerciseRepository = new ExerciseRepository(db);
        var muscleRepository = new MuscleRepository(db);

        //IExerciseService exerciseService = new ExerciseService(new ExerciseRepository(db), userRepository);
        //IMuscleService muscleService = new MuscleService(new MuscleRepository(db), userRepository);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        //Act
        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Plank", ExerciseType.Time,
            "Rectus abdominis", "External oblique", "Quadriceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Pull Up", ExerciseType.Reps,
            "Latissimus dorsi", "Biceps brachii", "Teres minor");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Push Up", ExerciseType.Reps,
            "Pectoralis major", "Triceps brachii", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Bench Press", ExerciseType.WeightAndReps,
            "Pectoralis major", "Triceps brachii", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Squat", ExerciseType.WeightAndReps,
            "Quadriceps", "Gluteus maximus", "Hamstrings");

        //Assert
        string plankExerciseStr = "Plank";
        Exercise? plankExercise = await exerciseRepository.GetByNameAsync(plankExerciseStr);

        Assert.NotNull(plankExercise);
        Assert.Equal(plankExercise?.Name, plankExerciseStr);
        Assert.Equal(plankExercise?.Type, ExerciseType.Time);

        string pushUpsExerciseStr = "Push Up";
        Exercise? pushUpsExercise = await exerciseRepository.GetByNameAsync(pushUpsExerciseStr);

        Assert.NotNull(pushUpsExercise);
        Assert.Equal(pushUpsExercise?.Name, pushUpsExerciseStr);
        Assert.Equal(pushUpsExercise?.Type, ExerciseType.Reps);

        var allExercises = await exerciseRepository.GetAllAsync();
        Assert.NotNull(allExercises);
        Assert.Contains(plankExercise, allExercises!);
        Assert.Contains(pushUpsExercise, allExercises!);
        Assert.Equal(5, allExercises?.Count());

    }
}
