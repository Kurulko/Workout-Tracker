using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;

namespace WorkoutTracker.Infrastructure.Initializers;

internal class UsersInitializer
{
    public static async Task<User> InitializeAsync(IUserRepository userRepository, string name, string email, string password, string[] rolesStr)
    {
        User? user = await userRepository.GetUserByUsernameAsync(name);

        if (user is null)
        {
            user = new User()
            {
                UserName = name,
                Email = email,
                Registered = DateTime.UtcNow
            };

            await userRepository.CreateUserAsync(user, password);
            await userRepository.AddRolesToUserAsync(user.Id, rolesStr);
        }

        return user;
    }
}