using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Infrastructure.Extensions;

internal static class AccountModelExtensions
{
    public static User ToUser(this LoginModel login)
    {
        ArgumentNullException.ThrowIfNull(login);

        return new User() { UserName = login.Name };
    }

    public static User ToUser(this RegisterModel register)
    {
        ArgumentNullException.ThrowIfNull(register);

        return new User() { Email = register.Email, UserName = register.Name };
    }
}
