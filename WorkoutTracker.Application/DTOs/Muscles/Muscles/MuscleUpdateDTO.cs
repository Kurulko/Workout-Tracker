using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.Application.DTOs.Muscles.Muscles;

public class MuscleUpdateDTO
{
    public long Id { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be longer than {1} characters")]
    [MaxLength(50, ErrorMessage = "{0} must be shorter than {1} characters")]
    public string Name { get; set; } = null!;

    public string? Image { get; set; }

    public long? ParentMuscleId { get; set; }
}
