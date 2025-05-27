using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Services.Exercises;

public interface IExerciseRecordService : IBaseService
{
    Task<ExerciseRecord?> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExerciseRecord>> GetUserExerciseRecordsAsync(string userId, long? exerciseId = null, ExerciseType? exerciseType = null, DateTimeRange? range = null, CancellationToken cancellationToken = default);

    Task<ExerciseRecord> AddExerciseRecordToExerciseRecordGroupAsync(long exerciseRecordGroupId, string userId, ExerciseRecord exerciseRecord, CancellationToken cancellationToken = default);
    Task UpdateUserExerciseRecordAsync(string userId, ExerciseRecord model, CancellationToken cancellationToken = default);

    Task DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId, CancellationToken cancellationToken = default);
}
