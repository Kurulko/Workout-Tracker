namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

public class EquipmentDetailsDTO
{
    public EquipmentDTO Equipment { get; set; } = null!;

    public IEnumerable<ChildMuscleDTO>? Muscles { get; set; }
    public IEnumerable<ExerciseDTO>? Exercises { get; set; }
}
