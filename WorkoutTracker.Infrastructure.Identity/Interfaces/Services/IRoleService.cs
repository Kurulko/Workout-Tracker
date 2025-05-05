using Microsoft.AspNetCore.Identity;

namespace WorkoutTracker.Infrastructure.Identity.Interfaces.Services;

public interface IRoleService
{
    Task<IdentityRole?> GetRoleByIdAsync(string key);
    Task<string?> GetRoleIdByNameAsync(string name);
    Task<string?> GetRoleNameByIdAsync(string key);
    Task<IdentityRole?> GetRoleByNameAsync(string name);
    Task<IQueryable<IdentityRole>> GetRolesAsync();
    Task<IdentityRole> AddRoleAsync(IdentityRole model);
    Task<IdentityResult> UpdateRoleAsync(IdentityRole model);
    Task<IdentityResult> DeleteRoleAsync(string key);
    Task<bool> RoleExistsAsync(string key);
    Task<bool> RoleExistsByNameAsync(string name);
}
