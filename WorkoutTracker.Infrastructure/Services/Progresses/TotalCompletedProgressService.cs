using WorkoutTracker.Application.Interfaces.Services.Progresses;
using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Application.Common.Extensions;

namespace WorkoutTracker.Infrastructure.Services.Progresses;

internal class TotalCompletedProgressService : ITotalCompletedProgressService
{
    public TotalCompletedProgress CalculateTotalCompletedProgress(IEnumerable<WorkoutRecord> workoutRecords)
    {
        if (workoutRecords is null || !workoutRecords.Any())
            return new TotalCompletedProgress();

        TotalCompletedProgress totalCompletedProgress = new();

        totalCompletedProgress.TotalWeightLifted = workoutRecords.GetTotalWeightValue();
        totalCompletedProgress.TotalRepsCompleted = workoutRecords.GetTotalRepsValue();
        totalCompletedProgress.TotalTimeCompleted = workoutRecords.GetTotalTimeValue();

        return totalCompletedProgress;
    }
}
