using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace WorkoutTrackerAPI.Data.DTOs;

public class MuscleDTO
{
    public long Id { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be longer than {1} characters")]
    [MaxLength(50, ErrorMessage = "{0} must be shorter than {1} characters")]
    public string Name { get; set; } = null!;

    public byte[]? Image { get; set; }

    public long? ParentMuscleId { get; set; }
    public Muscle? ParentMuscle { get; set; } = null!;
    public IEnumerable<Muscle>? ChildMuscles { get; set; } = null!;
}
