using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Models.Progresses;

namespace WorkoutTracker.Application.Interfaces.Services.Progresses;

public interface IWorkoutProgressService : IBaseService
{
    Task<CurrentUserProgress> CalculateCurrentUserProgressAsync(string userId, CancellationToken cancellationToken = default);

    Task<WorkoutProgress> CalculateWorkoutProgressAsync(string userId, DateTimeRange? range, CancellationToken cancellationToken = default);
    Task<WorkoutProgress> CalculateWorkoutProgressForYearAsync(string userId, int year, CancellationToken cancellationToken = default);
    Task<WorkoutProgress> CalculateWorkoutProgressForMonthAsync(string userId, YearMonth yearMonth, CancellationToken cancellationToken = default);
}
