using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Enums;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Services.ExerciseRecordServices;

public interface IExerciseRecordService
{
    Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId);
    Task<ServiceResult<IQueryable<ExerciseRecord>>> GetUserExerciseRecordsAsync(string userId, long? exerciseId = null, ExerciseType? exerciseType = null, DateTimeRange? range = null);
    Task<ServiceResult<ExerciseRecord>> AddExerciseRecordToUserAsync(string userId, ExerciseRecord exerciseRecord);
    Task<ServiceResult> UpdateUserExerciseRecordAsync(string userId, ExerciseRecord model);
    Task<ServiceResult> DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId);
}
