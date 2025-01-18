using WorkoutTrackerAPI.Data.Models;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace WorkoutTrackerAPI.Data.DTOs;

public class ExerciseDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Image { get; set; }
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }
    public bool IsCreatedByUser { get; set; }

    public IEnumerable<EquipmentDTO> Equipments { get; set; } = null!;

    [JsonProperty("muscles")]
    public IEnumerable<ChildMuscleDTO> WorkingMuscles { get; set; } = null!;
}
