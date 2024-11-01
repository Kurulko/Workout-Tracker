using System.ComponentModel.DataAnnotations.Schema;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public class WorkoutDTO : WorkoutModel
{
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime Started { get; set; }
    public int CountOfTrainings { get; set; }
    public double SumOfWeight { get; set; }
    public TimeSpan SumOfTime { get; set; }

    public IEnumerable<Exercise> Exercises { get; set; } = null!;
}
