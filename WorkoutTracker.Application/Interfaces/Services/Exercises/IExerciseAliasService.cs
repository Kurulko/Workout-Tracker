using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.Application.Interfaces.Services.Exercises;

public interface IExerciseAliasService
{
    Task<ServiceResult<ExerciseAlias>> GetExerciseAliasByIdAsync(long id);
    Task<ServiceResult<ExerciseAlias>> GetExerciseAliasByNameAsync(string name);

    Task<ServiceResult<IQueryable<ExerciseAlias>>> GetExerciseAliasesByExerciseIdAsync(long exerciseId);

    Task<ServiceResult<ExerciseAlias>> AddExerciseAliasToExerciseAsync(long exerciseId, ExerciseAlias exerciseAlias);
    Task<ServiceResult> UpdateExerciseAliasAsync(ExerciseAlias exerciseAlias);

    Task<ServiceResult> DeleteExerciseAliasAsync(long id);
}
