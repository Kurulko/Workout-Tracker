namespace WorkoutTrackerAPI.Data.DTOs;

public class MuscleDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public bool IsMeasurable { get; set; }
    public string? Image { get; set; }

    public long? ParentMuscleId { get; set; }
    public string? ParentMuscleName { get; set; }
    public IEnumerable<ChildMuscleDTO>? ChildMuscles { get; set; }
}

public class ChildMuscleDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
}