using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Validators;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTracker.Persistence.Repositories.Exercises;

internal class ExerciseAliasRepository : BaseWorkoutRepository<ExerciseAlias>, IExerciseAliasRepository
{
    public ExerciseAliasRepository(WorkoutDbContext db) : base(db)
    {

    }

    public async Task<ExerciseAlias> AddExerciseAliasAsync(long exerciseId, string aliasStr, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, nameof(Exercise));
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(aliasStr, nameof(aliasStr));

        return await AddAsync(exerciseId, aliasStr, cancellationToken);
    }

    public async Task AddExerciseAliasesAsync(long exerciseId, string[] aliasesStr, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, nameof(Exercise));

        foreach (var aliasStr in aliasesStr)
        {
            ArgumentValidator.ThrowIfArgumentNullOrEmpty(aliasStr, nameof(aliasStr));

            await AddAsync(exerciseId, aliasStr, cancellationToken);
        }
    }

    public IQueryable<ExerciseAlias> GetExerciseAliasesByExerciseId(long exerciseId)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, nameof(Exercise));

        return Find(er => er.ExerciseId == exerciseId);
    }

    public async Task RemoveByExerciseIdAsync(long exerciseId, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, nameof(Exercise));

        var aliases = await GetExerciseAliasesByExerciseId(exerciseId).ToListAsync(cancellationToken);

        if (aliases.Any())
            await RemoveRangeAsync(aliases, cancellationToken);
    }

    async Task<ExerciseAlias> AddAsync(long exerciseId, string aliasStr, CancellationToken cancellationToken)
    {
        var exerciseAlias = new ExerciseAlias
        {
            Name = aliasStr,
            ExerciseId = exerciseId
        };

        return await AddAsync(exerciseAlias, cancellationToken);
    }
}
