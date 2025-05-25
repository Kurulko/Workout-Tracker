using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Application.Interfaces.Services;

namespace WorkoutTracker.Infrastructure.Identity.Interfaces.Services;

public interface IRoleService : IBaseService
{
    Task<IdentityRole?> GetRoleByIdAsync(string key);
    Task<string?> GetRoleIdByNameAsync(string name);
    Task<IQueryable<IdentityRole>> GetRolesAsync();

    Task<string?> GetRoleNameByIdAsync(string key);
    Task<IdentityRole?> GetRoleByNameAsync(string name);

    Task<IdentityRole> AddRoleAsync(IdentityRole model);
    Task UpdateRoleAsync(IdentityRole model);

    Task DeleteRoleAsync(string key);
}
