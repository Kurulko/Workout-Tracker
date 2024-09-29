using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Runtime.Intrinsics.Arm;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Repositories;

public class RoleRepository
{
    readonly RoleManager<IdentityRole> roleManager;
    public RoleRepository(RoleManager<IdentityRole> roleManager)
        => this.roleManager = roleManager;

    IQueryable<IdentityRole> roles => roleManager.Roles;

    static IdentityResult roleNotFoundResult => IdentityResultExtentions.Failed("Role not found!");


    public async Task<IdentityRole> AddRoleAsync(IdentityRole role)
    {
        IdentityRole? existingRole = await GetRoleByIdAsync(role.Id);

        if (existingRole is null)
        {
            if (await RoleExistsByNameAsync(role.Name))
                throw new DbUpdateException("Role name must be unique.");

            await roleManager.CreateAsync(role);
            return role;
        }

        return existingRole;
    }

    public async Task<IdentityResult> DeleteRoleAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            return IdentityResultExtentions.Failed("Role ID cannot not be null or empty.");

        IdentityRole? role = await GetRoleByIdAsync(roleId);

        if (role is not null)
            return await roleManager.DeleteAsync(role);

        return roleNotFoundResult;
    }

    public async Task<IQueryable<IdentityRole>> GetRolesAsync()
        => await Task.FromResult(roles);

    public async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
        => await roles.SingleOrDefaultAsync(u => u.Id == roleId);

    public async Task<IdentityRole?> GetRoleByNameAsync(string name)
        => await roles.SingleOrDefaultAsync(u => u.Name == name);

    public async Task<IdentityResult> UpdateRoleAsync(IdentityRole role)
    {
        if (string.IsNullOrEmpty(role.Id))
            return IdentityResultExtentions.Failed("Role ID cannot not be null or empty.");

        IdentityRole? existingRole = await GetRoleByIdAsync(role.Id);

        if (existingRole is not null)
            return await roleManager.UpdateAsync(existingRole);

        return roleNotFoundResult;
    }

    public async Task<bool> RoleExistsAsync(string roleId)
        => await roles.AnyAsync(r => r.Id == roleId);

    public async Task<bool> RoleExistsByNameAsync(string name)
        => await roles.AnyAsync(r => r.Name == name);
}
