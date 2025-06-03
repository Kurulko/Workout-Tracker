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
}
