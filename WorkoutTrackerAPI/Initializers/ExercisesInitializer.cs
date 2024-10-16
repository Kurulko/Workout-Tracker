using System.Security.Cryptography;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Services.MuscleServices;

namespace WorkoutTrackerAPI.Initializers;

public class ExercisesInitializer
{
    public static async Task<Exercise> InitializeAsync(ExerciseRepository exerciseRepository, MuscleRepository muscleRepository, string name, ExerciseType exerciseType, params string[] muscleNames)
    {
        var exercise = await exerciseRepository.GetByNameAsync(name);

        if (exercise is not null)
            return exercise;

        exercise = new()
        {
            Name = name,
            Type = exerciseType
        };

        var muscles = new List<Muscle>();

        foreach (string muscleName in muscleNames)
        {
            var muscle = await muscleRepository.GetByNameAsync(muscleName);
            if (muscle is not null)
                muscles.Add(muscle);
        }

        exercise.WorkingMuscles = muscles;
        await exerciseRepository.AddAsync(exercise);

        return exercise;
    }
}
