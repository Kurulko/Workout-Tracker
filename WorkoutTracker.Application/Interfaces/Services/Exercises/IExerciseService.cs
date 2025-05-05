using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Services.Exercises;
public interface IExerciseService
{
    Task<ServiceResult<Exercise>> GetInternalExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false);
    Task<ServiceResult<Exercise>> GetUserExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false);
    Task<ServiceResult<Exercise>> GetExerciseByIdAsync(string userId, long exerciseId, bool withDetails = false);

    Task<ServiceResult<Exercise>> GetInternalExerciseByNameAsync(string userId, string name, bool withDetails = false);
    Task<ServiceResult<Exercise>> GetUserExerciseByNameAsync(string userId, string name, bool withDetails = false);
    Task<ServiceResult<Exercise>> GetExerciseByNameAsync(string userId, string name, bool withDetails = false);

    Task<ServiceResult<IQueryable<Exercise>>> GetInternalExercisesAsync(ExerciseType? exerciseType = null);
    Task<ServiceResult<IQueryable<Exercise>>> GetUserExercisesAsync(string userId, ExerciseType? exerciseType = null);
    Task<ServiceResult<IQueryable<Exercise>>> GetAllExercisesAsync(string userId, ExerciseType? exerciseType = null);
    Task<ServiceResult<IQueryable<Exercise>>> GetUsedExercisesAsync(string userId, ExerciseType? exerciseType = null);

    Task<ServiceResult<Exercise>> AddInternalExerciseAsync(Exercise model);
    Task<ServiceResult<Exercise>> AddUserExerciseAsync(string userId, Exercise model);

    Task<ServiceResult> UpdateInternalExerciseAsync(Exercise model);
    Task<ServiceResult> UpdateUserExerciseAsync(string userId, Exercise model);

    Task<ServiceResult> UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIDs);
    Task<ServiceResult> UpdateUserExerciseMusclesAsync(string userId, long exerciseId, IEnumerable<long> muscleIDs);

    Task<ServiceResult> UpdateInternalExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentIDs);
    Task<ServiceResult> UpdateUserExerciseEquipmentsAsync(string userId, long exerciseId, IEnumerable<long> equipmentIDs);

    Task<ServiceResult> DeleteInternalExerciseAsync(long exerciseId);
    Task<ServiceResult> DeleteExerciseFromUserAsync(string userId, long exerciseId);

    Task<bool> InternalExerciseExistsAsync(long exerciseId);
    Task<bool> UserExerciseExistsAsync(string userId, long exerciseId);

    Task<bool> InternalExerciseExistsByNameAsync(string name);
    Task<bool> UserExerciseExistsByNameAsync(string userId, string name);
}
