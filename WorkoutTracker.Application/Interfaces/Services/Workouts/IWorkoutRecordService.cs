using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.Application.Interfaces.Services.Workouts;

public interface IWorkoutRecordService : IBaseService
{
    Task<ServiceResult<WorkoutRecord>> GetUserWorkoutRecordByIdAsync(string userId, long workoutRecordId);
    Task<ServiceResult<IQueryable<WorkoutRecord>>> GetUserWorkoutRecordsAsync(string userId, long? workoutId = null, DateTimeRange? range = null);

    Task<ServiceResult<WorkoutRecord>> AddWorkoutRecordToUserAsync(string userId, WorkoutRecord workoutRecord);
    Task<ServiceResult> UpdateUserWorkoutRecordAsync(string userId, WorkoutRecord workoutRecord);

    Task<ServiceResult> DeleteWorkoutRecordFromUserAsync(string userId, long workoutRecordId);
}
