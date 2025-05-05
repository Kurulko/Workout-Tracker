using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.Common.Extensions.Enums;

public static class WorkoutStatusExtensions
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
