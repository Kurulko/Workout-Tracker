using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs;

public class UserCreationDTO : IEquatable<User>
{
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }

    public WeightType PreferableWeightType { get; set; }
    public SizeType PreferableSizeType { get; set; }

    [DateNotInFuture]
    public DateTime? StartedWorkingOut { get; set; }

    public bool Equals(User? other)
    {
        if (other == null)
            return false;

        return UserName == other.UserName && Email == other.Email;
    }

    public override bool Equals(object? obj)
    {
        return Equals((obj as User)!);
    }

    public override int GetHashCode()
        => HashCode.Combine(UserName, Email);

}
