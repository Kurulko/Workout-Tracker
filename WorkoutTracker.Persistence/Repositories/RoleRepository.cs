using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;

namespace WorkoutTracker.Persistence.Repositories;

internal class RoleRepository : IRoleRepository
{
    readonly RoleManager<IdentityRole> roleManager;
    public RoleRepository(RoleManager<IdentityRole> roleManager)
        => this.roleManager = roleManager;

    IQueryable<IdentityRole> Roles => roleManager.Roles;

    readonly string roleEntityName = "Role";

    public async Task<IdentityRole> AddRoleAsync(IdentityRole role)
    {
        IdentityRole? existingRole = await GetByIdAsync(role.Id);

        if (existingRole is null)
        {
            await ArgumentValidator.EnsureNonExistsByNameAsync(GetByNameAsync, role.Name!);
            
            var result = await roleManager.CreateAsync(role);
            ArgumentValidator.ThrowIfNotSucceeded("create", "role", result);

            return role;
        }

        return existingRole;
    }

    public async Task DeleteRoleAsync(string roleId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(roleId, roleEntityName);

        var role = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, roleId, roleEntityName);

        var result = await roleManager.DeleteAsync(role);
        ArgumentValidator.ThrowIfNotSucceeded("delete", "role", result);
    }

    public IQueryable<IdentityRole> GetRoles()
        => Roles;

    public async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(roleId, roleEntityName);

        return await GetByIdAsync(roleId);
    }

    public async Task<IdentityRole?> GetRoleByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(IdentityRole.Name));

        return await GetByNameAsync(name);
    }

    public async Task<string?> GetRoleIdByNameAsync(string name)
    {
        var roleByName = await GetRoleByIdAsync(name);
        return roleByName?.Id;
    }

    public async Task<string?> GetRoleNameByIdAsync(string roleId)
    {
        var roleById = await GetRoleByNameAsync(roleId);
        return roleById?.Name;
    }


    public async Task UpdateRoleAsync(IdentityRole role)
    {
        var existingRole = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, role.Id, roleEntityName);

        existingRole.Name = role.Name;

        var result = await roleManager.UpdateAsync(existingRole);
        ArgumentValidator.ThrowIfNotSucceeded("update", "role", result);
    }

    public async Task<bool> AnyAsync()
        => await Roles.AnyAsync();

    public async Task<bool> RoleExistsAsync(string roleId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(roleId, roleEntityName);

        return await Roles.AnyAsync(r => r.Id == roleId);
    }

    public async Task<bool> RoleExistsByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(IdentityRole.Name));

        return await Roles.AnyAsync(r => r.Name == name);
    }


    async Task<IdentityRole?> GetByIdAsync(string roleId)
        => await Roles.SingleOrDefaultAsync(u => u.Id == roleId);

    async Task<IdentityRole?> GetByNameAsync(string name)
        => await Roles.SingleOrDefaultAsync(u => u.Name == name);
}
