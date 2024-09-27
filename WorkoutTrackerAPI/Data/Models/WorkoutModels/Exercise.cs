using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data.Models;

public class Exercise : WorkoutModel
{
    public byte[]? Image { get; set; }
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }

    [NotMapped]
    public bool IsCreatedByUser => CreatedByUserId is not null;

    public string? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public IEnumerable<Muscle> WorkingMuscles { get; set; } = null!;
    public ICollection<ExerciseRecord>? ExerciseRecords { get; set; }
    public IEnumerable<Workout>? Workouts { get; set; }
}

public enum ExerciseType
{
    WeightAndTime,
    WeightAndReps,
    Time, 
    Reps
}
