namespace WorkoutTrackerAPI.Data.DTOs;

public struct TimeSpanModel
{
    public int Days { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int? Seconds { get; set; }
    public int? Milliseconds { get; set; }


    public static explicit operator TimeSpan(TimeSpanModel timeSpanModel)
    {
        return new TimeSpan(timeSpanModel.Days, timeSpanModel.Hours, timeSpanModel.Minutes, timeSpanModel.Seconds ?? 0, timeSpanModel.Milliseconds ?? 0);
    }

    public static explicit operator TimeSpanModel(TimeSpan timeSpan)
    {
        return new TimeSpanModel
        {
            Days = timeSpan.Days,
            Hours = timeSpan.Hours,
            Minutes = timeSpan.Minutes,
            Seconds = timeSpan.Seconds,
            Milliseconds = timeSpan.Milliseconds
        };
    }
}
