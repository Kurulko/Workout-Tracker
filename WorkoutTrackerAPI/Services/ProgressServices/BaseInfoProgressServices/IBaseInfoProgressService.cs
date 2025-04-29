using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Services.ProgressServices.BaseInfoProgressServices;

public interface IBaseInfoProgressService
{
    BaseInfoProgress CalculateBaseInfoProgress(IEnumerable<WorkoutRecord> workoutRecords, DateTimeRange range);
}
