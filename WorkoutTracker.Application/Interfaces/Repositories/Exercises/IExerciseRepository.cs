using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Repositories.Exercises;

public interface IExerciseRepository : IBaseWorkoutRepository<Exercise>
{
    Task<Exercise?> GetExerciseByIdWithDetailsAsync(long key, string userId, CancellationToken cancellationToken = default);
    Task<Exercise?> GetExerciseByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken = default);

    IQueryable<Exercise> GetInternalExercises(ExerciseType? exerciseType = null, string? filterQuery = null);
    IQueryable<Exercise> GetUserExercises(string userId, ExerciseType? exerciseType = null, string? filterQuery = null);
    IQueryable<Exercise> GetAllExercises(string userId, ExerciseType? exerciseType = null, string? filterQuery = null);

    Task<string?> GetExercisePhotoAsync(long key, CancellationToken cancellationToken = default);
    Task UpdateExercisePhotoAsync(long key, string image, CancellationToken cancellationToken = default);
    Task DeleteExercisePhotoAsync(long key, CancellationToken cancellationToken = default);

    IQueryable<Exercise> FilterByExerciseType(IQueryable<Exercise> exercises, ExerciseType? exerciseType);
    IQueryable<Exercise> FilterByQuery(IQueryable<Exercise> exercises, string? filterQuery);

}