using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.ProgressModels;

namespace WorkoutTrackerAPI.Extentions;

public static class WorkoutStatusExtentions
{
    public static WorkoutStatus GetWorkoutStatusByYears(int countOfYears)
    {
        return countOfYears switch
        {
            < 1 => WorkoutStatus.Beginner,
            < 5 => WorkoutStatus.Intermediate,
            < 10 => WorkoutStatus.Advanced,
            _ => WorkoutStatus.Elite
        };
    }

    public static WorkoutStatus GetWorkoutStatusByDays(int countOfDays)
    {
        const int daysInYear = 365;
        int countOfYears = countOfDays / daysInYear;
        return GetWorkoutStatusByYears(countOfYears);
    }
}
