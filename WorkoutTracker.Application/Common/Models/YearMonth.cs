namespace WorkoutTracker.Application.Common.Models;

public record struct YearMonth
{
    public int Year { get; init; }
    public int Month { get; init; }

    public YearMonth(int year, int month)
    {
        if (year <= 0)
        {
            throw new ArgumentException("Year must be a positive value.");
        }

        if (month <= 0 || month > 12)
        {
            throw new ArgumentException("Month must be a positive value and less than 12.");
        }

        Year = year;
        Month = month;
    }
}
