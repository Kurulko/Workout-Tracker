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

    public async Task<MuscleAlias> AddMuscleAliasAsync(long muscleId, string aliasStr, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(muscleId, nameof(Muscle));
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(aliasStr, nameof(aliasStr));

        return await AddAsync(muscleId, aliasStr, cancellationToken);
    }

    public async Task AddMuscleAliasesAsync(long muscleId, string[] aliasesStr, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(muscleId, nameof(Muscle));

        foreach (var aliasStr in aliasesStr)
        {
            ArgumentValidator.ThrowIfArgumentNullOrEmpty(aliasStr, nameof(aliasStr));

            await AddAsync(muscleId, aliasStr, cancellationToken);
        }
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

    async Task<MuscleAlias> AddAsync(long muscleId, string aliasStr, CancellationToken cancellationToken)
    {
        var muscleAlias = new MuscleAlias
        {
            Name = aliasStr,
            MuscleId = muscleId
        };

        return await AddAsync(muscleAlias, cancellationToken);
    }
}
