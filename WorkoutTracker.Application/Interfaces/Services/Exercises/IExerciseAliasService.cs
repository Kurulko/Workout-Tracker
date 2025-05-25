using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.Application.Interfaces.Services.Exercises;

public interface IExerciseAliasService : IBaseService
{
    Task<ExerciseAlias?> GetExerciseAliasByIdAsync(long id);
    Task<ExerciseAlias?> GetExerciseAliasByNameAsync(string name);

    Task<IQueryable<ExerciseAlias>> GetExerciseAliasesByExerciseIdAsync(long exerciseId);

    Task<ExerciseAlias> AddExerciseAliasToExerciseAsync(long exerciseId, ExerciseAlias exerciseAlias);
    Task UpdateExerciseAliasAsync(ExerciseAlias exerciseAlias);

    Task DeleteExerciseAliasAsync(long id);
}
