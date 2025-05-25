using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Services.Exercises;

public interface IExerciseService : IBaseService
{
    Task<Exercise?> GetInternalExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false);
    Task<Exercise?> GetUserExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false);
    Task<Exercise?> GetExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false);
                 
    Task<Exercise?> GetInternalExerciseByNameAsync(string userId, string name, bool withDetails = false);
    Task<Exercise?> GetUserExerciseByNameAsync(string userId, string name, bool withDetails = false);
    Task<Exercise?> GetExerciseByNameAsync(string userId, string name, bool withDetails = false);

    Task<IQueryable<Exercise>> GetInternalExercisesAsync(ExerciseType? exerciseType = null);
    Task<IQueryable<Exercise>> GetUserExercisesAsync(string userId, ExerciseType? exerciseType = null);
    Task<IQueryable<Exercise>> GetAllExercisesAsync(string userId, ExerciseType? exerciseType = null);
    Task<IQueryable<Exercise>> GetUsedExercisesAsync(string userId, ExerciseType? exerciseType = null);

    Task<Exercise> AddInternalExerciseAsync(Exercise model);
    Task<Exercise> AddUserExerciseAsync(string userId, Exercise model);

    Task UpdateInternalExerciseAsync(Exercise model);
    Task UpdateUserExerciseAsync(string userId, Exercise model);

    Task UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIDs);
    Task UpdateUserExerciseMusclesAsync(string userId, long exerciseId, IEnumerable<long> muscleIDs);

    Task UpdateInternalExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentIDs);
    Task UpdateUserExerciseEquipmentsAsync(string userId, long exerciseId, IEnumerable<long> equipmentIDs);

    Task DeleteInternalExerciseAsync(long exerciseId);
    Task DeleteExerciseFromUserAsync(string userId, long exerciseId);
}
