using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Services.WorkoutRecordServices;

public interface IWorkoutRecordService
{
    Task<ServiceResult<WorkoutRecord>> GetUserWorkoutRecordByIdAsync(string userId, long workoutRecordId);
    Task<ServiceResult<IQueryable<WorkoutRecord>>> GetUserWorkoutRecordsAsync(string userId, long? workoutId = null, DateTime? date = null);
    Task<ServiceResult<WorkoutRecord>> AddWorkoutRecordToUserAsync(string userId, WorkoutRecord workoutRecord);
    Task<ServiceResult> UpdateUserWorkoutRecordAsync(string userId, WorkoutRecord workoutRecord);
    Task<ServiceResult> DeleteWorkoutRecordFromUserAsync(string userId, long workoutRecordId);
}
