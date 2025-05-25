using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Services.Exercises;

public interface IExerciseRecordService : IBaseService
{
    Task<ExerciseRecord?> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId);
    Task<IQueryable<ExerciseRecord>> GetUserExerciseRecordsAsync(string userId, long? exerciseId = null, ExerciseType? exerciseType = null, DateTimeRange? range = null);

    Task<ExerciseRecord> AddExerciseRecordToExerciseRecordGroupAsync(long exerciseRecordGroupId, string userId, ExerciseRecord exerciseRecord);
    Task UpdateUserExerciseRecordAsync(string userId, ExerciseRecord model);

    Task DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId);
}
