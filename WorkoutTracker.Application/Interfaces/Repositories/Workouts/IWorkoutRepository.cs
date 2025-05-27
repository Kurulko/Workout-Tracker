using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Workouts;

public interface IWorkoutRepository : IBaseWorkoutRepository<Workout>
{
    Task<Workout?> GetWorkoutByIdWithDetailsAsync(long key, CancellationToken cancellationToken = default);
    Task<Workout?> GetWorkoutByNameWithDetailsAsync(string name, CancellationToken cancellationToken = default);

    IQueryable<Workout> GetUserWorkouts(string userId, long? exerciseId = null);

    Task IncreaseCountOfWorkoutsAsync(long workoutId, CancellationToken cancellationToken = default);
    Task DecreaseCountOfWorkoutsAsync(long workoutId, CancellationToken cancellationToken = default);
}