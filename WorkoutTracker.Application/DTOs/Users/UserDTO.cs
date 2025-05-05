using WorkoutTracker.Application.Common.ValidationAttributes;
using WorkoutTracker.Domain.Entities.Users;

namespace WorkoutTracker.Application.DTOs.Users;

public class UserDTO //: IEquatable<User>
{
    public string UserId { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string? Email { get; set; }

    [DateNotInFuture]
    public DateTime Registered { get; set; }

    [DateNotInFuture]
    public DateTime? StartedWorkingOut { get; set; }

    //public bool Equals(User? other)
    //{
    //    if (other == null)
    //        return false;

    //    return UserId == other.Id && UserName == other.UserName && Email == other.Email;
    //}

    //public override bool Equals(object? obj)
    //{
    //    return Equals((obj as User)!);
    //}

    public override int GetHashCode()
        => HashCode.Combine(UserId, UserName, Email);
}
