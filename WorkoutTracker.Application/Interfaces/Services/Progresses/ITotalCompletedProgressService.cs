using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Application.Interfaces.Services.Progresses;

public interface ITotalCompletedProgressService
{
    TotalCompletedProgress CalculateTotalCompletedProgress(IEnumerable<WorkoutRecord> workoutRecords);
}
