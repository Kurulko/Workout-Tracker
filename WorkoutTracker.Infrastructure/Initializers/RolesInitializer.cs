using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Application.Interfaces.Repositories;

namespace WorkoutTracker.Infrastructure.Initializers;

internal class RolesInitializer
{
    public static async Task InitializeAsync(IRoleRepository roleRepository, params string[] rolesStr)
    {
        foreach (string roleStr in rolesStr)
        {
            IdentityRole? role = await roleRepository.GetRoleByNameAsync(roleStr);

            if (role is null)
                await roleRepository.AddRoleAsync(new IdentityRole(roleStr));
        }
    }
}