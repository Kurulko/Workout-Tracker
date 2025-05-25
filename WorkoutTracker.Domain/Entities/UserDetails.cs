using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Domain.Entities;

public class UserDetails : IDbModel
{
    public long Id { get; set; }
    public Gender? Gender { get; set; }
    public ModelSize? Height { get; set; }
    public ModelWeight? Weight { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public double? BodyFatPercentage { get; set; }

    public string UserId { get; set; } = null!;
}

