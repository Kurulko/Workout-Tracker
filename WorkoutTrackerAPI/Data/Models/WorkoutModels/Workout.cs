using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Data.Models;

public class Workout : WorkoutModel
{
    public string? Description { get; set; }
    public DateTime Created { get; set; }
    public bool IsPinned { get; set; }
    public int CountOfTrainings { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }

    public IEnumerable<ExerciseSetGroup>? ExerciseSetGroups { get; set; }
    public IEnumerable<WorkoutRecord>? WorkoutRecords { get; set; }
}