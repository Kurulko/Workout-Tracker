using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

public class ExerciseSetGroupCreationDTO
{
    public long ExerciseId { get; set; }
    public long WorkoutId { get; set; }

    public IEnumerable<ExerciseSetCreationDTO> ExerciseSets { get; set; } = null!;
}
