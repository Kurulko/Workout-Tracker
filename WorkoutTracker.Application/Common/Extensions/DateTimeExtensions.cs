using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.Application.Common.Extensions;

public static class DateTimeExtensions
{
    public static DateTime[] GetDatesBetween(DateTime startDate, DateTime endDate)
    {
        var dates = new List<DateTime>();

        for (DateTime currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
        {
            dates.Add(currentDate);
        }

        return dates.ToArray();
    }

    public static DateTime[] GetDatesBetween(this DateTimeRange range)
        => GetDatesBetween(range.FirstDate, range.LastDate);

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
