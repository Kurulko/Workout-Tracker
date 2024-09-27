using Microsoft.AspNetCore.Identity;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.UserServices;

namespace WorkoutTrackerAPI.Initializers;

public class UsersInitializer
{
    public static async Task InitializeAsync(UserRepository userRepository, string name, string email, string password, params string[] rolesStr)
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
                foreach (string roleStr in rolesStr)
                {
                    await userRepository.AddRoleToUserAsync(user.Id, roleStr);
                }
            }
        }
    }
}