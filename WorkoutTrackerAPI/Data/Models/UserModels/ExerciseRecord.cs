﻿namespace WorkoutTrackerAPI.Data.Models.UserModels;

public class ExerciseRecord : IDbModel
{
    public long Id { get; set; }
    public DateTime Date { get; set; }

    public ModelWeight? Weight { get; set; }
    public TimeSpan? Time { get; set; }
    public int? Reps { get; set; }

    public long ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    public long ExerciseRecordGroupId { get; set; }
    public ExerciseRecordGroup? ExerciseRecordGroup { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }
}
