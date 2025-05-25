using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.Application.Interfaces.Services.Workouts;

public interface IWorkoutRecordService : IBaseService
{
    Task<WorkoutRecord?> GetUserWorkoutRecordByIdAsync(string userId, long workoutRecordId);
    Task<IQueryable<WorkoutRecord>> GetUserWorkoutRecordsAsync(string userId, long? workoutId = null, DateTimeRange? range = null);

    Task<WorkoutRecord> AddWorkoutRecordToUserAsync(string userId, WorkoutRecord workoutRecord);
    Task UpdateUserWorkoutRecordAsync(string userId, WorkoutRecord workoutRecord);

    Task DeleteWorkoutRecordFromUserAsync(string userId, long workoutRecordId);
}
