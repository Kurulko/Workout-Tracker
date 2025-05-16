using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Users;

public class UserDetailsDTO
{
    public Gender? Gender { get; set; }
    public ModelSize? Height { get; set; }
    public ModelWeight? Weight { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public double? BodyFatPercentage { get; set; }
}
