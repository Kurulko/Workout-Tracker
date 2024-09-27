using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkoutTrackerAPI.Data.Models;

public class Workout : WorkoutModel
{
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime Started { get; set; }
    public int CountOfTrainings { get; set; }
    public double SumOfWeight { get; set; }
    public TimeSpan SumOfTime { get; set; }

    [NotMapped]
    public IEnumerable<Muscle>? MuscledUsed { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }

    public IEnumerable<Exercise> Exercises { get; set; } = null!;
}
