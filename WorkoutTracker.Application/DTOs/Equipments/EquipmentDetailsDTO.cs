using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;

namespace WorkoutTracker.Application.DTOs.Equipments;

public class EquipmentDetailsDTO
{
    public EquipmentDTO Equipment { get; set; } = null!;

    public IEnumerable<ChildMuscleDTO>? Muscles { get; set; }
    public IEnumerable<ExerciseDTO>? Exercises { get; set; }
}
