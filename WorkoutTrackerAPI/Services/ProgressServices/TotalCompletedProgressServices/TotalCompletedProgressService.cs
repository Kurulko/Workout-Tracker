using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services.ProgressServices.TotalCompletedProgressServices;

namespace WorkoutTrackerAPI.Services.WorkoutProgressServices;

public class TotalCompletedProgressService : ITotalCompletedProgressService
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
