using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Services.Exercises;

public interface IExerciseRecordService : IBaseService
{
    Task<ServiceResult<ExerciseRecord>> GetUserExerciseRecordByIdAsync(string userId, long exerciseRecordId);
    Task<ServiceResult<IQueryable<ExerciseRecord>>> GetUserExerciseRecordsAsync(string userId, long? exerciseId = null, ExerciseType? exerciseType = null, DateTimeRange? range = null);
    Task<ServiceResult<ExerciseRecord>> AddExerciseRecordToUserAsync(string userId, ExerciseRecord exerciseRecord);
    Task<ServiceResult> UpdateUserExerciseRecordAsync(string userId, ExerciseRecord model);
    Task<ServiceResult> DeleteExerciseRecordFromUserAsync(string userId, long exerciseRecordId);
}
