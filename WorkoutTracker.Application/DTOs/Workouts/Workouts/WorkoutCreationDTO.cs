using WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSetGroups;

namespace WorkoutTracker.Application.DTOs.Workouts.Workouts;

public class WorkoutCreationDTO
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    public IEnumerable<ExerciseSetGroupDTO> ExerciseSetGroups { get; set; } = null!;
}
