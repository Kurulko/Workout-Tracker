using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Workouts;

public interface IWorkoutRepository : IBaseWorkoutRepository<Workout>
{
    Task<Workout?> GetWorkoutByIdWithDetailsAsync(long key);
    Task<Workout?> GetWorkoutByNameWithDetailsAsync(string name);
}