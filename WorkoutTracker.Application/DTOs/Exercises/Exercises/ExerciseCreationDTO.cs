using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.DTOs.Exercises.Exercises;

public class ExerciseCreationDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }
}
