using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

namespace WorkoutTrackerAPI.Initializers;

public static class WorkoutInitializer
{
    public static async Task<Workout> InitializeWorkoutAsync(WorkoutRepository workoutRepository, string workoutName, string userId)
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

    public static async Task<WorkoutRecord> InitializeWorkoutRecordAsync(WorkoutRecordRepository workoutRecordRepository, string userId, long workoutId, DateTime date, TimeSpan time)
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

    public static async Task<ExerciseRecordGroup> InitializeExerciseRecordGroupAsync(ExerciseRecordGroupRepository exerciseRecordGroupRepository, long exerciseId, long workoutRecordId)
    {
        var exerciseRecordGroup = new ExerciseRecordGroup()
        {
            ExerciseId = exerciseId,
            WorkoutRecordId = workoutRecordId,
        };

        return await exerciseRecordGroupRepository.AddAsync(exerciseRecordGroup);
    }

    public static async Task InitializeExerciseRecordAsync(ExerciseRecordRepository exerciseRecordRepository, DateTime date, long exerciseId, long exerciseRecordGroupId, string userId, int sets = 1, int? reps = null, ModelWeight? weight = null, TimeSpan? time = null)
    {
        for (int i = 0; i < sets; i++)
        {
            var exerciseRecord = new ExerciseRecord()
            {
                Date = date,
                ExerciseId = exerciseId,
                ExerciseRecordGroupId = exerciseRecordGroupId,
                UserId = userId,
                Weight = weight,
                Time = time,
                Reps = reps
            };

            await exerciseRecordRepository.AddAsync(exerciseRecord);
        }
    }

    public static async Task<ExerciseSetGroup> InitializeExerciseSetGroupAsync(ExerciseSetGroupRepository exerciseSetGroupRepository, long exerciseId, long workoutId)
    {
        var exerciseSetGroup = new ExerciseSetGroup()
        {
            ExerciseId = exerciseId,
            WorkoutId = workoutId,
        };

        return await exerciseSetGroupRepository.AddAsync(exerciseSetGroup);
    }

    public static async Task InitializeExerciseSetAsync(ExerciseSetRepository exerciseSetRepository, long exerciseId, long exerciseSetGroupId, string userId, int sets = 1, int? reps = null, ModelWeight? weight = null, TimeSpan? time = null)
    {
        for (int i = 0; i < sets; i++)
        {
            var exerciseSet = new ExerciseSet()
            {
                ExerciseId = exerciseId,
                ExerciseSetGroupId = exerciseSetGroupId,
                UserId = userId,
                Weight = weight,
                Time = time,
                Reps = reps
            };

            await exerciseSetRepository.AddAsync(exerciseSet);
        }
    }

}
