using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.ProgressModels;

namespace WorkoutTrackerAPI.Services.ProgressServices;

public interface IWorkoutProgressService
{
    Task<CurrentUserProgress> CalculateCurrentUserProgressAsync(string userId);

    Task<WorkoutProgress> CalculateWorkoutProgressAsync(string userId, DateTimeRange? range);
    Task<WorkoutProgress> CalculateWorkoutProgressForYearAsync(string userId, int year);
    Task<WorkoutProgress> CalculateWorkoutProgressForMonthAsync(string userId, YearMonth yearMonth);
}
