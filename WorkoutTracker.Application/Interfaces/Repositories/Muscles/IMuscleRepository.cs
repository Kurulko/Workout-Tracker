using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Muscles;

public interface IMuscleRepository : IBaseWorkoutRepository<Muscle>
{
    Task<Muscle?> GetMuscleByIdWithDetailsAsync(long key, string userId);
    Task<Muscle?> GetMuscleByNameWithDetailsAsync(string name, string userId);
}