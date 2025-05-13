using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;

namespace WorkoutTracker.Domain.Entities.Workouts;

public class Workout : BaseWorkoutModel
{
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public bool IsPinned { get; set; }
    public int CountOfTrainings { get; set; }

    public string UserId { get; set; } = null!;
    public IEnumerable<ExerciseSetGroup>? ExerciseSetGroups { get; set; }
    public IEnumerable<WorkoutRecord>? WorkoutRecords { get; set; }
}