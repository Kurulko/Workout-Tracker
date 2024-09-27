using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Services.ExerciseServices;

public interface IExerciseService
{
    Task<ServiceResult<Exercise>> GetExerciseByIdAsync(long exerciseId);
    Task<ServiceResult<Exercise>> GetUserExerciseByIdAsync(string userId, long exerciseId);

    Task<ServiceResult<Exercise>> GetExerciseByNameAsync(string name);
    Task<ServiceResult<Exercise>> GetUserExerciseByNameAsync(string userId, string name);

    Task<ServiceResult<IQueryable<Exercise>>> GetExercisesAsync();
    Task<ServiceResult<IQueryable<Exercise>>> GetUserExercisesAsync(string userId);

    Task<ServiceResult<Exercise>> AddExerciseAsync(Exercise model);
    Task<ServiceResult<Exercise>> AddUserExerciseAsync(string userId, Exercise model);

    Task<ServiceResult> UpdateExerciseAsync(Exercise model);
    Task<ServiceResult> UpdateUserExerciseAsync(string userId, Exercise model);

    Task<ServiceResult> DeleteExerciseAsync(long exerciseId);
    Task<ServiceResult> DeleteExerciseFromUserAsync(string userId, long exerciseId);

    Task<bool> ExerciseExistsAsync(long exerciseId);
    Task<bool> UserExerciseExistsAsync(string userId, long exerciseId);

    Task<bool> ExerciseExistsByNameAsync(string name);
    Task<bool> UserExerciseExistsByNameAsync(string userId, string name);
}
