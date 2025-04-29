using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.ExerciseAliasDTOs;

public class ExerciseAliasDTO
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long ExerciseId { get; set; }
}
