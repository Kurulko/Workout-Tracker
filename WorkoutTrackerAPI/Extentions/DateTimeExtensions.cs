using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Extentions;

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
}
