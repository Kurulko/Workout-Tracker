using WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSets;

namespace WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSetGroups;

public class ExerciseSetGroupCreationDTO
{
    public long ExerciseId { get; set; }
    public long WorkoutId { get; set; }

    public IEnumerable<ExerciseSetCreationDTO> ExerciseSets { get; set; } = null!;
}
