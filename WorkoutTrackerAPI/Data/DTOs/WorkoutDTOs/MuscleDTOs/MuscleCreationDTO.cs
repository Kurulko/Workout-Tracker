using System.ComponentModel.DataAnnotations;

namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.MuscleDTOs;

public class MuscleCreationDTO
{
    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be longer than {1} characters")]
    [MaxLength(50, ErrorMessage = "{0} must be shorter than {1} characters")]
    public string Name { get; set; } = null!;
    public IFormFile? ImageFile { get; set; }

    public long? ParentMuscleId { get; set; }
}
