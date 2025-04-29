using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Services.ProgressServices.WorkoutWeightLiftedProgressServices;

public interface IWorkoutWeightLiftedProgressService
{
    WorkoutWeightLiftedProgress CalculateWorkoutWeightLiftedProgress(IEnumerable<WorkoutRecord> workoutRecords);
}
