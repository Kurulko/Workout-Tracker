using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Persistence.Repositories.Base;

namespace WorkoutTracker.Persistence.Repositories.Muscles;

internal class MuscleAliasRepository : BaseWorkoutRepository<MuscleAlias>, IMuscleAliasRepository
{
    public MuscleAliasRepository(WorkoutDbContext db) : base(db)
    {

    }

    public IQueryable<MuscleAlias> GetMuscleAliasesByMuscleId(long muscleId)
    {
        ArgumentValidator.ThrowIfIdNonPositive(muscleId, nameof(Muscle));

        return Find(ma => ma.MuscleId == muscleId);
    }

    public async Task RemoveByMuscleIdAsync(long muscleId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(muscleId, nameof(Muscle));

        var aliases = await GetMuscleAliasesByMuscleId(muscleId).ToListAsync(cancellationToken);

        if (aliases.Any())
            await RemoveRangeAsync(aliases, cancellationToken);
    }
}
