using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data.DTOs.UserDTOs;

public class UserDetailsDTO
{
    public Gender? Gender { get; set; }
    public ModelSize? Height { get; set; }
    public ModelWeight? Weight { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public double? BodyFatPercentage { get; set; }
}
