using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Application.DTOs.Exercises.Exercises;

public class ExerciseCreationDTO
{
    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be longer than {1} characters")]
    [MaxLength(50, ErrorMessage = "{0} must be shorter than {1} characters")]
    public string Name { get; set; } = null!;

    public FileUploadModel? ImageFile { get; set; }
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }
}
