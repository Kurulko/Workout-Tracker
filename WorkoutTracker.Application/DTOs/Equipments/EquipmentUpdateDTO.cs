namespace WorkoutTracker.Application.DTOs.Equipments;

public class EquipmentUpdateDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Image { get; set; }
}
