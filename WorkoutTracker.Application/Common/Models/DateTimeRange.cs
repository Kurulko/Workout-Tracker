namespace WorkoutTracker.Application.Common.Models;

public record class DateTimeRange
{
    public DateTime FirstDate { get; init; }
    public DateTime LastDate { get; init; }
    public int CountOfDays { get; init; }

    public DateTimeRange(DateTime firstDate, DateTime lastDate)
    {
        if (firstDate > lastDate)
        {
            throw new ArgumentException("First date must be earlier than or equal to the last date.");
        }

        FirstDate = firstDate;
        LastDate = lastDate;
        CountOfDays = (int)(lastDate - firstDate).TotalDays + 1;
    }


    public static DateTimeRange GetRangeRangeByYear(int year, bool isTillToday = true)
    {
        var firstDateForYear = new DateTime(year, 1, 1);
        var lastDateForYear = firstDateForYear.AddYears(1).AddDays(-1);

        if (isTillToday && lastDateForYear > DateTime.Today)
        {
            lastDateForYear = DateTime.Today;
        }

        return new DateTimeRange(firstDateForYear, lastDateForYear);
    }

    public static DateTimeRange GetRangeRangeByMonth(YearMonth yearMonth, bool isTillToday = true)
    {
        var firstDateForMonth = new DateTime(yearMonth.Year, yearMonth.Month, 1);
        var lastDateForMonth = firstDateForMonth.AddMonths(1).AddDays(-1);

        if (isTillToday && lastDateForMonth > DateTime.Today)
        {
            lastDateForMonth = DateTime.Today;
        }

        return new DateTimeRange(firstDateForMonth, lastDateForMonth);
    }
}