using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Application.Interfaces.Repositories.Workouts;

public interface IWorkoutRecordRepository : IBaseRepository<WorkoutRecord>
{
    IQueryable<WorkoutRecord> GetUserWorkoutRecords(string userId, long? workoutId = null, DateTimeRange? range = null);
}