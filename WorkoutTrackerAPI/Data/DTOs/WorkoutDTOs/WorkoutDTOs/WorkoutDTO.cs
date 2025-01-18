using Newtonsoft.Json;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

namespace WorkoutTrackerAPI.Data.DTOs;

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
