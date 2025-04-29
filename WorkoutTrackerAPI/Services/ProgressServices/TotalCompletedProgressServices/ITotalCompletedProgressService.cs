using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Services.ProgressServices.TotalCompletedProgressServices;

public interface ITotalCompletedProgressService
{
    TotalCompletedProgress CalculateTotalCompletedProgress(IEnumerable<WorkoutRecord> workoutRecords);
}
