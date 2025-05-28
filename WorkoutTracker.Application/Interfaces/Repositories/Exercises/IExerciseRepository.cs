using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Repositories.Exercises;

public interface IExerciseRepository : IBaseWorkoutRepository<Exercise>
{
    Task<Exercise?> GetExerciseByIdWithDetailsAsync(long key, string userId, CancellationToken cancellationToken = default);
    Task<Exercise?> GetExerciseByNameWithDetailsAsync(string name, string userId, CancellationToken cancellationToken = default);

    IQueryable<Exercise> GetInternalExercises(ExerciseType? exerciseType = null);
    IQueryable<Exercise> GetUserExercises(string userId, ExerciseType? exerciseType = null);
    IQueryable<Exercise> GetAllExercises(string userId, ExerciseType? exerciseType = null);
}