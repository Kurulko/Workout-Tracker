namespace WorkoutTracker.Application.DTOs.Muscles.Muscles;

public class MuscleCreationDTO
{
    public string Name { get; set; } = null!;
    public long? ParentMuscleId { get; set; }
}