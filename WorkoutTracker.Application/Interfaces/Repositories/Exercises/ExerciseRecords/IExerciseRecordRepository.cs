using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;

public interface IExerciseRecordRepository : IBaseRepository<ExerciseRecord>
{
    Task<IQueryable<ExerciseRecord>> GetExerciseRecordsByUserIdAsync(string userId);
    Task<string?> GetUserIdByExerciseRecordIdAsync(long exerciseRecordId);
}
