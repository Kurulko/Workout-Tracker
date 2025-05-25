namespace WorkoutTracker.Application.DTOs.Muscles.Muscles;

public class MuscleUpdateDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Image { get; set; }

    public long? ParentMuscleId { get; set; }
}
