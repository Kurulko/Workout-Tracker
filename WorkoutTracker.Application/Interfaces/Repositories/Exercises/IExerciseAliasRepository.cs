using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Interfaces.Repositories.Exercises;

public interface IExerciseAliasRepository : IBaseWorkoutRepository<ExerciseAlias>
{
    IQueryable<ExerciseAlias> GetExerciseAliasesByExerciseId(long exerciseId);

    Task<ExerciseAlias> AddExerciseAliasAsync(long exerciseId, string aliasStr, CancellationToken cancellationToken = default);
    Task AddExerciseAliasesAsync(long exerciseId, string[] aliasesStr, CancellationToken cancellationToken = default);

    Task RemoveByExerciseIdAsync(long exerciseId, CancellationToken cancellationToken = default);
}
