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

    IQueryable<IdentityRole> Roles => roleManager.Roles;

    static IdentityResult RoleNotFoundResult => IdentityResultExtentions.Failed("Role not found.");
    static IdentityResult RoleIDIsNullOrEmptyResult => IdentityResultExtentions.Failed("Role ID cannot not be null or empty.");


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
            return RoleIDIsNullOrEmptyResult;

        IdentityRole? role = await GetRoleByIdAsync(roleId);

        if (role is not null)
            return await roleManager.DeleteAsync(role);

        return RoleNotFoundResult;
    }

    public virtual async Task<IQueryable<IdentityRole>> GetRolesAsync()
        => await Task.FromResult(Roles);

    public virtual async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
        => await Roles.SingleOrDefaultAsync(u => u.Id == roleId);

    public virtual async Task<IdentityRole?> GetRoleByNameAsync(string name)
        => await Roles.SingleOrDefaultAsync(u => u.Name == name);

    public virtual async Task<IdentityResult> UpdateRoleAsync(IdentityRole role)
    {
        if (string.IsNullOrEmpty(role.Id))
            return RoleIDIsNullOrEmptyResult;

        IdentityRole? existingRole = await GetRoleByIdAsync(role.Id);

        if (existingRole is not null)
            return await roleManager.UpdateAsync(existingRole);

        return RoleNotFoundResult;
    }

    public virtual async Task<bool> AnyAsync()
        => await Roles.AnyAsync();

    public virtual async Task<bool> RoleExistsAsync(string roleId)
        => await Roles.AnyAsync(r => r.Id == roleId);

    public virtual async Task<bool> RoleExistsByNameAsync(string name)
        => await Roles.AnyAsync(r => r.Name == name);
}
