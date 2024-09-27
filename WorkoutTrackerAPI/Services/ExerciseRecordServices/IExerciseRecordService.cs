using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Services.ExerciseRecordServices;

public interface IExerciseRecordService
{
    Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId);
    Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByDateAsync(string userId, long exerciseId, DateOnly date);
    Task<ServiceResult<IQueryable<ExerciseRecord>>> GetUserExerciseRecordsAsync(string userId, long exerciseId);
    Task<ServiceResult<ExerciseRecord>> AddExerciseRecordToUserAsync(string userId, ExerciseRecord exerciseRecord);
    Task<ServiceResult> UpdateUserExerciseRecordAsync(string userId, ExerciseRecord model);
    Task<ServiceResult> DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId);
}
