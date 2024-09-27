using Microsoft.AspNetCore.Identity;

namespace WorkoutTrackerAPI.Data.Models.UserModels;

public class Role : IdentityRole, IBaseModel
{
    //public override bool Equals(object? obj)
    //    => obj is Role role && Name == role.Name;

    //public override int GetHashCode()
    //    => Name.GetHashCode();
}
