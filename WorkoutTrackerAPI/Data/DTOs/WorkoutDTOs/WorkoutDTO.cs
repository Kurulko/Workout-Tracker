using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs;

public class WorkoutDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [DateNotInFuture]
    public DateTime Created { get; set; }

    [DateNotInFuture]
    public DateTime Started { get; set; }

    [PositiveNumber]
    public int CountOfTrainings { get; set; }

    [PositiveNumber]
    public double Weight { get; set; }
    public TimeSpan Time { get; set; }

    [PositiveNumber]
    public double SumOfWeight { get; set; }
    public TimeSpan SumOfTime { get; set; }

    public IEnumerable<Exercise> Exercises { get; set; } = null!;
}
