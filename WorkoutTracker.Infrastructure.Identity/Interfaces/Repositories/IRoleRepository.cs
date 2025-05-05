using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Extensions;

namespace WorkoutTracker.Application.Interfaces.Repositories;

public interface IRoleRepository
{

    Task<IdentityRole> AddRoleAsync(IdentityRole role);
    Task<IdentityResult> UpdateRoleAsync(IdentityRole role);

    Task<IdentityResult> DeleteRoleAsync(string roleId);

    Task<IQueryable<IdentityRole>> GetRolesAsync();
    Task<IdentityRole?> GetRoleByIdAsync(string roleId);
    Task<IdentityRole?> GetRoleByNameAsync(string name);

    Task<bool> AnyAsync();
    Task<bool> RoleExistsAsync(string roleId);
    Task<bool> RoleExistsByNameAsync(string name);
}
