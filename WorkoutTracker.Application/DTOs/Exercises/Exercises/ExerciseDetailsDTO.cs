using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Exercises.Exercises;

public class ExerciseDetailsDTO
{
    public ExerciseDTO Exercise { get; set; } = null!;

    public int CountOfTimes { get; set; }

    public ModelWeight? SumOfWeight { get; set; }
    public TimeSpanModel? SumOfTime { get; set; }
    public int? SumOfReps { get; set; }

    public IEnumerable<DateTime>? Dates { get; set; }
}