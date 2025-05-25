using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSetGroups;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Workouts.Workouts;

public class WorkoutDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public DateTime? Started { get; set; }
    public bool IsPinned { get; set; }
    public ModelWeight Weight { get; set; }

    public IEnumerable<ExerciseDTO> Exercises { get; set; } = null!;
    public IEnumerable<ExerciseSetGroupDTO> ExerciseSetGroups { get; set; } = null!;
}
