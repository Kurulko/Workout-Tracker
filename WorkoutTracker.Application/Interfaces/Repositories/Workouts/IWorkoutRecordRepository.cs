using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Application.Interfaces.Repositories.Workouts;

public interface IWorkoutRecordRepository : IBaseRepository<WorkoutRecord>
{
}