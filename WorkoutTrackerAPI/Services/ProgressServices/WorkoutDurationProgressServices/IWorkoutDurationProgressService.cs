using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Services.ProgressServices.WorkoutDurationProgressServices;

public interface IWorkoutDurationProgressService
{
    WorkoutDurationProgress CalculateWorkoutDurationProgress(IEnumerable<WorkoutRecord> workoutRecords, BaseInfoProgress baseInfoProgress);
}
