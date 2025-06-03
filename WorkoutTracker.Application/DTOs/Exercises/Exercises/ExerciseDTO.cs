using Newtonsoft.Json;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;

namespace WorkoutTracker.Application.DTOs.Exercises.Exercises;

public class ExerciseDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Image { get; set; }
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }
    public bool IsCreatedByUser { get; set; }
    public string[] Aliases { get; set; } = null!;

    public IEnumerable<EquipmentDTO> Equipments { get; set; } = null!;

    [JsonProperty("muscles")]
    public IEnumerable<ChildMuscleDTO> WorkingMuscles { get; set; } = null!;
}
