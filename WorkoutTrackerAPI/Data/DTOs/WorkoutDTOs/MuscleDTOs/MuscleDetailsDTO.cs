using System.ComponentModel.DataAnnotations;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

public class MuscleDetailsDTO
{
    public MuscleDTO Muscle { get; set; } = null!;

    public IEnumerable<Exercise>? Exercises { get; set; }
    public IEnumerable<MuscleSize>? MuscleSizes { get; set; }
}
