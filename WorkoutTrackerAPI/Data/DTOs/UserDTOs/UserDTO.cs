using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs;

public class UserDTO : IEquatable<User>
{
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }

    public WeightType PreferableWeightType { get; set; }
    public SizeType PreferableSizeType { get; set; }

    [DateNotInFuture]
    public DateTime Registered { get; set; }

    [DateNotInFuture]
    public DateTime? StartedWorkingOut { get; set; }

    public bool Equals(User? other)
    {
        if (other == null)
            return false;

        return UserId == other.Id && UserName == other.UserName && Email == other.Email;
    }

    public override bool Equals(object? obj)
    {
        return Equals((obj as User)!);
    }

    public override int GetHashCode()
        => HashCode.Combine(UserId, UserName, Email);
}
