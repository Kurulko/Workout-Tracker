using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Repositories;

public interface IBaseWorkoutRepository<T> : IBaseRepository<T>
    where T : WorkoutModel
{
    Task<T?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
}
