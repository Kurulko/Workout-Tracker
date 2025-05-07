using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.Application.DTOs.Muscles.Muscles;

public class MuscleCreationDTO
{
    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be longer than {1} characters")]
    [MaxLength(50, ErrorMessage = "{0} must be shorter than {1} characters")]
    public string Name { get; set; } = null!;

    public long? ParentMuscleId { get; set; }
}
