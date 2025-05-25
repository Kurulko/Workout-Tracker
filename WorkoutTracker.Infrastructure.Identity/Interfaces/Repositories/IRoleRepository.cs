using Microsoft.AspNetCore.Identity;

namespace WorkoutTracker.Application.Interfaces.Repositories;

public interface IRoleRepository
{
    Task<IdentityRole> AddRoleAsync(IdentityRole role);
    Task UpdateRoleAsync(IdentityRole role);

    Task DeleteRoleAsync(string roleId);

    Task<IQueryable<IdentityRole>> GetRolesAsync();
    Task<IdentityRole?> GetRoleByIdAsync(string roleId);
    Task<IdentityRole?> GetRoleByNameAsync(string name);

    Task<bool> AnyAsync();
    Task<bool> RoleExistsAsync(string roleId);
    Task<bool> RoleExistsByNameAsync(string name);
}
