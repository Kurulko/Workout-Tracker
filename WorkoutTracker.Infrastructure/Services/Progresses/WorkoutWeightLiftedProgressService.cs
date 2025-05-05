using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.Interfaces.Services.Progresses;
using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Infrastructure.Services.Progresses;

internal class WorkoutWeightLiftedProgressService : IWorkoutWeightLiftedProgressService
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
