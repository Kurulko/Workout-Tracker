using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Muscles;

public interface IMuscleRepository : IBaseWorkoutRepository<Muscle>
{
    Task<Muscle?> GetMuscleByIdWithDetailsAsync(long key, string userId, CancellationToken cancellationToken = default);
    Task<Muscle?> GetMuscleByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken = default);

    IQueryable<Muscle> GetMuscles(long? parentMuscleId = null, bool? isMeasurable = null);
    IQueryable<Muscle> GetParentMuscles();
    IQueryable<Muscle> GetChildMuscles();

    IQueryable<Muscle> FindByIds(IEnumerable<long> muscleIds);
}