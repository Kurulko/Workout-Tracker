namespace WorkoutTrackerAPI.Data.DTOs;

public class EquipmentDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Image { get; set; }
    public bool IsOwnedByUser { get; set; }
}
