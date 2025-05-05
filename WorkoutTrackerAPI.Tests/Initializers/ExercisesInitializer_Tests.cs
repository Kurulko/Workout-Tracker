using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data.Models.UserModels;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Services.MuscleServices;
using WorkoutTracker.API.Services;
using WorkoutTracker.API.Services.RoleServices;
using Xunit;
using WorkoutTracker.API.Services.ExerciseServices;
using WorkoutTracker.API.Repositories.UserRepositories;

namespace WorkoutTracker.API.Tests.Initializers;

public class ExercisesInitializer_Tests
{
    [Fact]
    public static async Task InitializeAsync()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var exerciseRepository = new ExerciseRepository(db);
        var muscleRepository = new MuscleRepository(db);

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
