using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services.ProgressServices.WorkoutWeightLiftedProgressServices;

namespace WorkoutTrackerAPI.Services.ProgressServices;

public class WorkoutWeightLiftedProgressService : IWorkoutWeightLiftedProgressService
{
    public WorkoutWeightLiftedProgress CalculateWorkoutWeightLiftedProgress(IEnumerable<WorkoutRecord> workoutRecords)
    {
        if (!workoutRecords.Any())
            return new WorkoutWeightLiftedProgress();

        WorkoutWeightLiftedProgress workoutWeightLiftedProgress = new();

        IEnumerable<ModelWeight> weightsLifted = workoutRecords.Select(wr => wr.ExerciseRecordGroups.GetTotalWeightValue()!)!;

        var totalWorkouts = workoutRecords.Count();
        var totalWeightLifted = workoutRecords.GetTotalWeightValue();

        workoutWeightLiftedProgress.AverageWorkoutWeightLifted = totalWeightLifted / totalWorkouts;
        workoutWeightLiftedProgress.MinWorkoutWeightLifted = weightsLifted.Min();
        workoutWeightLiftedProgress.MaxWorkoutWeightLifted = weightsLifted.Max();

        return workoutWeightLiftedProgress;
    }
}
