using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Base;

public interface IBaseWorkoutRepository<T> : IBaseRepository<T>
    where T : BaseWorkoutModel
{
    Task<T?> GetByNameAsync(string name);
    Task<bool> ExistsByNameAsync(string name);
}
