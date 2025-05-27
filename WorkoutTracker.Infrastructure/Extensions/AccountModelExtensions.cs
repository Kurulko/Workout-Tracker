using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Infrastructure.Extensions;

internal static class AccountModelExtensions
{
    public static User ToUser(this LoginModel login)
        => new User() { UserName = login.Name };

    public static User ToUser(this RegisterModel register)
        => new User() { Email = register.Email, UserName = register.Name };
}
