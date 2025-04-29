using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.ProgressModels;

namespace WorkoutTrackerAPI.Services.ProgressServices.StrikeDurationProgressServices;

public interface IStrikeDurationProgressService
{
    StrikeDurationProgress CalculateStrikeDurationProgress(BaseInfoProgress baseInfoProgress, DateTimeRange range);
}
