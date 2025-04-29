using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Services.ProgressServices.WorkoutDurationProgressServices;

namespace WorkoutTrackerAPI.Services.ProgressServices;

public class WorkoutDurationProgressService : IWorkoutDurationProgressService
{
    public WorkoutDurationProgress CalculateWorkoutDurationProgress(IEnumerable<WorkoutRecord> workoutRecords, BaseInfoProgress baseInfoProgress)
    {
        if (!workoutRecords.Any())
            return new WorkoutDurationProgress();

        WorkoutDurationProgress workoutDurationProgress = new();

        var totalDuration = baseInfoProgress.TotalDuration;
        var totalWorkouts = baseInfoProgress.TotalWorkouts;

        workoutDurationProgress.AverageWorkoutDuration = TimeSpan.FromMinutes(totalDuration.TotalMinutes / totalWorkouts);
        workoutDurationProgress.MinWorkoutDuration = workoutRecords.MinBy(wr => wr.Time)!.Time;
        workoutDurationProgress.MaxWorkoutDuration = workoutRecords.MaxBy(wr => wr.Time)!.Time;

        return workoutDurationProgress;
    }
}
