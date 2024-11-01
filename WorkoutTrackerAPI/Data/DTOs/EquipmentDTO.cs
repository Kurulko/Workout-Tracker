using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public class EquipmentDTO : WorkoutModel
{
    public byte[]? Image { get; set; }
    public bool IsOwnedByUser { get; set; }
    public IEnumerable<Exercise>? Exercises { get; set; } = null!;
}
