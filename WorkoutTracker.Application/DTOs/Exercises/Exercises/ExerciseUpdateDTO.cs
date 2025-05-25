using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.DTOs.Exercises.Exercises;

public class ExerciseUpdateDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;

    public string? Image { get; set; }
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }
}
