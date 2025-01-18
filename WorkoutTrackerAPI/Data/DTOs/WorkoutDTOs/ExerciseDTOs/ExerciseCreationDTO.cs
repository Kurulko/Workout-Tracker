using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.ExerciseDTOs;

public class ExerciseCreationDTO
{
    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be longer than {1} characters")]
    [MaxLength(50, ErrorMessage = "{0} must be shorter than {1} characters")]
    public string Name { get; set; } = null!;

    public IFormFile? ImageFile { get; set; }
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }
}
