using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Models.Progresses;

namespace WorkoutTracker.Application.Interfaces.Services.Progresses;

public interface IWorkoutProgressService : IBaseService
{
    Task<CurrentUserProgress> CalculateCurrentUserProgressAsync(string userId);

    Task<WorkoutProgress> CalculateWorkoutProgressAsync(string userId, DateTimeRange? range);
    Task<WorkoutProgress> CalculateWorkoutProgressForYearAsync(string userId, int year);
    Task<WorkoutProgress> CalculateWorkoutProgressForMonthAsync(string userId, YearMonth yearMonth);
}
