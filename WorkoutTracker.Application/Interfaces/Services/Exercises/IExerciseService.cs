using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Services.Exercises;

public interface IExerciseService : IBaseService
{
    #region Internal Exercises

    Task<Exercise?> GetInternalExerciseByIdAsync(string userId, long exerciseId, CancellationToken cancellationToken = default);
    Task<Exercise?> GetInternalExerciseByNameAsync(string userId, string name, CancellationToken cancellationToken = default);
    
    Task<Exercise?> GetInternalExerciseByIdWithDetailsAsync(string userId, long exerciseId, CancellationToken cancellationToken = default);
    Task<Exercise?> GetInternalExerciseByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken = default);

    Task<IEnumerable<Exercise>> GetInternalExercisesAsync(ExerciseType? exerciseType = null, CancellationToken cancellationToken = default);

    Task<Exercise> AddInternalExerciseAsync(Exercise model, CancellationToken cancellationToken = default);
    Task UpdateInternalExerciseAsync(Exercise model, CancellationToken cancellationToken = default);

    Task UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIDs, CancellationToken cancellationToken = default);
    Task UpdateInternalExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentIDs, CancellationToken cancellationToken = default);

    Task DeleteInternalExerciseAsync(long exerciseId, CancellationToken cancellationToken = default);

    #endregion

    #region User Exercises

    Task<Exercise?> GetUserExerciseByIdAsync(string userId, long exerciseId, CancellationToken cancellationToken = default);
    Task<Exercise?> GetUserExerciseByNameAsync(string userId, string name, CancellationToken cancellationToken = default);

    Task<Exercise?> GetUserExerciseByIdWithDetailsAsync(string userId, long exerciseId, CancellationToken cancellationToken = default);
    Task<Exercise?> GetUserExerciseByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken = default);

    Task<IEnumerable<Exercise>> GetUserExercisesAsync(string userId, ExerciseType? exerciseType = null, CancellationToken cancellationToken = default);

    Task<Exercise> AddUserExerciseAsync(string userId, Exercise model, CancellationToken cancellationToken = default);
    Task UpdateUserExerciseAsync(string userId, Exercise model, CancellationToken cancellationToken = default);

    Task UpdateUserExerciseMusclesAsync(string userId, long exerciseId, IEnumerable<long> muscleIDs, CancellationToken cancellationToken = default);
    Task UpdateUserExerciseEquipmentsAsync(string userId, long exerciseId, IEnumerable<long> equipmentIDs, CancellationToken cancellationToken = default);

    Task DeleteExerciseFromUserAsync(string userId, long exerciseId, CancellationToken cancellationToken = default);

    #endregion

    #region All Exercises

    Task<Exercise?> GetExerciseByIdAsync(string userId, long exerciseId, CancellationToken cancellationToken = default);
    Task<Exercise?> GetExerciseByNameAsync(string userId, string name, CancellationToken cancellationToken = default);

    Task<Exercise?> GetExerciseByIdWithDetailsAsync(string userId, long exerciseId, CancellationToken cancellationToken = default);
    Task<Exercise?> GetExerciseByNameWithDetailsAsync(string userId, string name, CancellationToken cancellationToken = default);

    Task<IEnumerable<Exercise>> GetAllExercisesAsync(string userId, ExerciseType? exerciseType = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Exercise>> GetUsedExercisesAsync(string userId, ExerciseType? exerciseType = null, CancellationToken cancellationToken = default);

    #endregion
}
