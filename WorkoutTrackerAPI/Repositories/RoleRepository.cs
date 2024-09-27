using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;

namespace WorkoutTrackerAPI.Repositories;

public class RoleRepository
{
    readonly RoleManager<Role> roleManager;
    public RoleRepository(RoleManager<Role> roleManager)
        => this.roleManager = roleManager;

    IQueryable<Role> roles => roleManager.Roles;

    IdentityResult roleNotFoundResult => IdentityResult.Failed(new IdentityError()
    {
        Description = "Role not found!"
    });


    public async Task<Role> AddRoleAsync(Role role)
    {
        Role? existingRole = await GetRoleByIdAsync(role.Id);
        if (existingRole is null)
        {
            await roleManager.CreateAsync(role);
            return role;
        }
        return existingRole;
    }

    public async Task<IdentityResult> DeleteRoleAsync(string roleId)
    {
        Role? role = await GetRoleByIdAsync(roleId);

        if (role is not null)
            return await roleManager.DeleteAsync(role);

        return roleNotFoundResult;
    }

    public async Task<IQueryable<Role>> GetRolesAsync()
        => await Task.FromResult(roles);

    public async Task<Role?> GetRoleByIdAsync(string roleId)
        => await roles.SingleOrDefaultAsync(u => u.Id == roleId);

    public async Task<Role?> GetRoleByNameAsync(string name)
        => await roles.SingleOrDefaultAsync(u => u.Name == name);

    public async Task<IdentityResult> UpdateRoleAsync(Role role)
    {
        Role? existingRole = await GetRoleByIdAsync(role.Id);

        if (existingRole is not null)
            return await roleManager.UpdateAsync(existingRole);

        return roleNotFoundResult;
    }

    public async Task<string?> GetRoleIdByNameAsync(string name)
        => (await GetRoleByNameAsync(name))?.Id;

    public async Task<string?> GetRoleNameByIdAsync(string roleId)
        => (await GetRoleByIdAsync(roleId))?.Name;

    public async Task<bool> RoleExistsAsync(string roleId)
        => await roles.AnyAsync(r => r.Id == roleId);

    public async Task<bool> RoleExistsByNameAsync(string name)
        => await roles.AnyAsync(r => r.Name == name);
}
