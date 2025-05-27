using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.Application.Interfaces.Services.Workouts;

public interface IWorkoutRecordService : IBaseService
{
    Task<WorkoutRecord?> GetUserWorkoutRecordByIdAsync(string userId, long workoutRecordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkoutRecord>> GetUserWorkoutRecordsAsync(string userId, long? workoutId = null, DateTimeRange? range = null, CancellationToken cancellationToken = default);

    Task<WorkoutRecord> AddWorkoutRecordToUserAsync(string userId, WorkoutRecord workoutRecord, CancellationToken cancellationToken = default);
    Task UpdateUserWorkoutRecordAsync(string userId, WorkoutRecord workoutRecord, CancellationToken cancellationToken = default);

    Task DeleteWorkoutRecordFromUserAsync(string userId, long workoutRecordId, CancellationToken cancellationToken = default);
}
