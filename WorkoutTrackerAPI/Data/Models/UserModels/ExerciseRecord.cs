﻿namespace WorkoutTrackerAPI.Data.Models.UserModels;

public class ExerciseRecord : IDbModel
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public int CountOfTimes { get; set; }

    public double? Weight { get; set; }
    public TimeSpan? Time { get; set; }
    public int? Reps { get; set; }

    public double? SumOfWeight { get; set; }
    public TimeSpan? SumOfTime { get; set; }
    public int? SumOfReps { get; set; }

    public long ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }
}
