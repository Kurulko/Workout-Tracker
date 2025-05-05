using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSetGroups;

namespace WorkoutTracker.Application.DTOs.Workouts.Workouts;

public class WorkoutCreationDTO
{
    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be longer than {1} characters")]
    [MaxLength(50, ErrorMessage = "{0} must be shorter than {1} characters")]
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public IEnumerable<ExerciseSetGroupDTO> ExerciseSetGroups { get; set; } = null!;
}
