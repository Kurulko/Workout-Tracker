using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
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

    static IdentityResult roleNotFoundResult => IdentityResultExtentions.Failed("Role not found.");
    static IdentityResult roleIDIsNullOrEmptyResult => IdentityResultExtentions.Failed("Role ID cannot not be null or empty.");


    public virtual async Task<IdentityRole> AddRoleAsync(IdentityRole role)
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

    public virtual async Task<IdentityResult> DeleteRoleAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            return roleIDIsNullOrEmptyResult;

        IdentityRole? role = await GetRoleByIdAsync(roleId);

        if (role is not null)
            return await roleManager.DeleteAsync(role);

        return roleNotFoundResult;
    }

    public virtual async Task<IQueryable<IdentityRole>> GetRolesAsync()
        => await Task.FromResult(roles);

    public virtual async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
        => await roles.SingleOrDefaultAsync(u => u.Id == roleId);

    public virtual async Task<IdentityRole?> GetRoleByNameAsync(string name)
        => await roles.SingleOrDefaultAsync(u => u.Name == name);

    public virtual async Task<IdentityResult> UpdateRoleAsync(IdentityRole role)
    {
        if (string.IsNullOrEmpty(role.Id))
            return roleIDIsNullOrEmptyResult;

        IdentityRole? existingRole = await GetRoleByIdAsync(role.Id);

        if (existingRole is not null)
            return await roleManager.UpdateAsync(existingRole);

        return roleNotFoundResult;
    }

    public virtual async Task<bool> RoleExistsAsync(string roleId)
        => await roles.AnyAsync(r => r.Id == roleId);

    public virtual async Task<bool> RoleExistsByNameAsync(string name)
        => await roles.AnyAsync(r => r.Name == name);
}
