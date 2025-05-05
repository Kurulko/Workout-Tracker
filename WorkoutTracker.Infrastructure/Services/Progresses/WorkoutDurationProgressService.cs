using WorkoutTracker.Application.Interfaces.Services.Progresses;
using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Infrastructure.Services.Progresses;

internal class WorkoutDurationProgressService : IWorkoutDurationProgressService
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
