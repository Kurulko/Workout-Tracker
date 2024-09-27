using Microsoft.AspNetCore.Identity;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Services.RoleServices;

public interface IRoleService
{
    Task<Role?> GetRoleByIdAsync(string key);
    Task<string?> GetRoleIdByNameAsync(string name);
    Task<string?> GetRoleNameByIdAsync(string key);
    Task<Role?> GetRoleByNameAsync(string name);
    Task<IQueryable<Role>> GetRolesAsync();
    Task<Role> AddRoleAsync(Role model);
    Task<IdentityResult> UpdateRoleAsync(Role model);
    Task<IdentityResult> DeleteRoleAsync(string key);
    Task<bool> RoleExistsAsync(string key);
    Task<bool> RoleExistsByNameAsync(string name);
}
