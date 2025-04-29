using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Extentions;

public static class DateTimeRangeExtensions
{
    public static bool IsDateInRange(this DateTimeRange range, DateTime date, bool isIncluding = true)
    {
        if (isIncluding)
        {
            return date >= range.FirstDate && date <= range.LastDate;
        }
        else
        {
            return date > range.FirstDate && date < range.LastDate;
        }
    }
}
