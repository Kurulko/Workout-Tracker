using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;

public interface IExerciseRecordRepository : IBaseRepository<ExerciseRecord>
{
    Task<string?> GetUserIdByExerciseRecordIdAsync(long exerciseRecordId, CancellationToken cancellationToken = default);

    IQueryable<ExerciseRecord> GetUserExerciseRecords(string userId, long? exerciseId = null, ExerciseType? exerciseType = null, DateTimeRange? range = null);

}
