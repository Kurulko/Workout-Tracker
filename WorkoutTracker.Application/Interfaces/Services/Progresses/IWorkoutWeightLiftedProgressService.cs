using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Application.Interfaces.Services.Progresses;

public interface IWorkoutWeightLiftedProgressService
{
    WorkoutWeightLiftedProgress CalculateWorkoutWeightLiftedProgress(IEnumerable<WorkoutRecord> workoutRecords);
}
