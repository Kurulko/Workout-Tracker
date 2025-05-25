using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Application.Interfaces.Services.Progresses;

public interface IWorkoutDurationProgressService : IBaseService
{
    WorkoutDurationProgress CalculateWorkoutDurationProgress(IEnumerable<WorkoutRecord> workoutRecords, BaseInfoProgress baseInfoProgress);
}
