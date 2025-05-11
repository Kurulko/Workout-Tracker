using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Application.Interfaces.Services.Progresses;

public interface IBaseInfoProgressService : IBaseService
{
    BaseInfoProgress CalculateBaseInfoProgress(IEnumerable<WorkoutRecord> workoutRecords, DateTimeRange range);
}
