using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Infrastructure.Initializers;

internal static class WorkoutInitializer
{
    public static async Task<Workout> InitializeWorkoutAsync(IWorkoutRepository workoutRepository, string workoutName, string userId)
    {
        var workout = await workoutRepository.GetByNameAsync(workoutName);

        if (workout is not null)
            return workout;

        workout = new Workout()
        {
            Name = workoutName,
            Created = DateTime.Now,
            UserId = userId
        };

        return await workoutRepository.AddAsync(workout);
    }

    public static async Task<WorkoutRecord> InitializeWorkoutRecordAsync(IWorkoutRecordRepository workoutRecordRepository, string userId, long workoutId, DateTime date, TimeSpan time)
    {
        var workoutRecord = new WorkoutRecord()
        {
            Time = time,
            Date = date,
            WorkoutId = workoutId,
            UserId = userId
        };

        return await workoutRecordRepository.AddAsync(workoutRecord);
    }

    public static async Task<ExerciseRecordGroup> InitializeExerciseRecordGroupAsync(IExerciseRecordGroupRepository exerciseRecordGroupRepository, long exerciseId, long workoutRecordId)
    {
        var exerciseRecordGroup = new ExerciseRecordGroup()
        {
            ExerciseId = exerciseId,
            WorkoutRecordId = workoutRecordId,
        };

        return await exerciseRecordGroupRepository.AddAsync(exerciseRecordGroup);
    }

    public static async Task InitializeExerciseRecordAsync(IExerciseRecordRepository exerciseRecordRepository, DateTime date, long exerciseId, long exerciseRecordGroupId, string userId, int sets = 1, int? reps = null, ModelWeight? weight = null, TimeSpan? time = null)
    {
        for (int i = 0; i < sets; i++)
        {
            var exerciseRecord = new ExerciseRecord()
            {
                Date = date,
                ExerciseId = exerciseId,
                ExerciseRecordGroupId = exerciseRecordGroupId,
                Weight = weight,
                Time = time,
                Reps = reps
            };

            await exerciseRecordRepository.AddAsync(exerciseRecord);
        }
    }

    public static async Task<ExerciseSetGroup> InitializeExerciseSetGroupAsync(IExerciseSetGroupRepository exerciseSetGroupRepository, long exerciseId, long workoutId)
    {
        var exerciseSetGroup = new ExerciseSetGroup()
        {
            ExerciseId = exerciseId,
            WorkoutId = workoutId,
        };

        return await exerciseSetGroupRepository.AddAsync(exerciseSetGroup);
    }

    public static async Task InitializeExerciseSetAsync(IExerciseSetRepository exerciseSetRepository, long exerciseId, long exerciseSetGroupId, string userId, int sets = 1, int? reps = null, ModelWeight? weight = null, TimeSpan? time = null)
    {
        for (int i = 0; i < sets; i++)
        {
            var exerciseSet = new ExerciseSet()
            {
                ExerciseId = exerciseId,
                ExerciseSetGroupId = exerciseSetGroupId,
                Weight = weight,
                Time = time,
                Reps = reps
            };

            await exerciseSetRepository.AddAsync(exerciseSet);
        }
    }
}
