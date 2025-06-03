using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Interfaces.Repositories.Muscles;

public interface IMuscleAliasRepository : IBaseWorkoutRepository<MuscleAlias>
{
    IQueryable<MuscleAlias> GetMuscleAliasesByMuscleId(long muscleId);

    Task RemoveByMuscleIdAsync(long muscleId, CancellationToken cancellationToken = default);
}
