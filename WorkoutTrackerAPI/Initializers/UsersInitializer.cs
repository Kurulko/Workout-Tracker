using Microsoft.AspNetCore.Identity;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.UserServices;

namespace WorkoutTrackerAPI.Initializers;

public class UsersInitializer
{
    public static async Task<User> InitializeAsync(UserRepository userRepository, string name, string email, string password, string[] rolesStr)
    {
        User? user = await userRepository.GetUserByUsernameAsync(name);

        if (user is null)
        {
            user = new User()
            {
                UserName = name,
                Email = email,
                Registered = DateTime.Now
            };

            IdentityResult result = await userRepository.CreateUserAsync(user, password);

            if (result.Succeeded)
            {
                await userRepository.AddRolesToUserAsync(user.Id, rolesStr);
            }
        }

        return user;
    }
}