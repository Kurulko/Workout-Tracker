using System.ComponentModel.DataAnnotations.Schema;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public class ExerciseDTO : WorkoutModel
{
    public byte[]? Image { get; set; }
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }

    public bool IsCreatedByUser { get; set; }

    public IEnumerable<Muscle> WorkingMuscles { get; set; } = null!;
}
