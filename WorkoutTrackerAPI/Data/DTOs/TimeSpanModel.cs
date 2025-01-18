using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public struct TimeSpanModel
{
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }
    public int Milliseconds { get; set; }


    public static explicit operator TimeSpan(TimeSpanModel timeSpanModel)
    {
        return new TimeSpan(0, timeSpanModel.Hours, timeSpanModel.Minutes, timeSpanModel.Seconds, timeSpanModel.Milliseconds);
    }

    public static explicit operator TimeSpanModel(TimeSpan timeSpan)
    {
        return new TimeSpanModel
        {
            Hours = timeSpan.Hours,
            Minutes = timeSpan.Minutes,
            Seconds = timeSpan.Seconds,
            Milliseconds = timeSpan.Milliseconds
        };
    }
}
