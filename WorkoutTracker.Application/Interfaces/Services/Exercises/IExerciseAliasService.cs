using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.Application.Interfaces.Services.Exercises;

public interface IExerciseAliasService : IBaseService
{
    Task<ExerciseAlias?> GetExerciseAliasByIdAsync(long id, CancellationToken cancellationToken = default);
    Task<ExerciseAlias?> GetExerciseAliasByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IEnumerable<ExerciseAlias>> GetExerciseAliasesByExerciseIdAsync(long exerciseId, CancellationToken cancellationToken = default);

    Task<ExerciseAlias> AddExerciseAliasToExerciseAsync(long exerciseId, ExerciseAlias exerciseAlias, CancellationToken cancellationToken = default);
    Task UpdateExerciseAliasAsync(ExerciseAlias exerciseAlias, CancellationToken cancellationToken = default);

    Task DeleteExerciseAliasAsync(long id, CancellationToken cancellationToken = default);
}
