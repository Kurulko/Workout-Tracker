using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Identity.Extensions;

namespace WorkoutTracker.Persistence.Repositories;

internal class RoleRepository : IRoleRepository
{
    readonly RoleManager<IdentityRole> roleManager;
    public RoleRepository(RoleManager<IdentityRole> roleManager)
        => this.roleManager = roleManager;

    IQueryable<IdentityRole> Roles => roleManager.Roles;

    readonly string roleEntityName = "Role";

    public virtual async Task<IdentityRole> AddRoleAsync(IdentityRole role)
    {
        IdentityRole? existingRole = await GetByIdAsync(role.Id);

        if (existingRole is null)
        {
            await ArgumentValidator.EnsureNonExistsByNameAsync(GetByNameAsync, role.Name!);
            
            var result = await roleManager.CreateAsync(role);

            if (!result.Succeeded)
                throw new ValidationException($"Failed to add role: {result.IdentityErrorsToString()}");

            return role;
        }

        return existingRole;
    }

    public virtual async Task DeleteRoleAsync(string roleId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(roleId, roleEntityName);

        var role = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, roleId, roleEntityName);

        var result = await roleManager.DeleteAsync(role);

        if (!result.Succeeded)
            throw new ValidationException($"Failed to delete role: {result.IdentityErrorsToString()}");
    }

    public virtual async Task<IQueryable<IdentityRole>> GetRolesAsync()
        => await Task.FromResult(Roles);

    public virtual async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(roleId, roleEntityName);

        return await GetByIdAsync(roleId);
    }

    public virtual async Task<IdentityRole?> GetRoleByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(IdentityRole.Name));

        return await GetByNameAsync(name);
    }

    public virtual async Task UpdateRoleAsync(IdentityRole role)
    {
        var existingRole = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, role.Id, roleEntityName);

        existingRole.Name = role.Name;

        var result = await roleManager.UpdateAsync(existingRole);

        if (!result.Succeeded)
            throw new ValidationException($"Failed to update role: {result.IdentityErrorsToString()}");
    }

    public virtual async Task<bool> AnyAsync()
        => await Roles.AnyAsync();

    public virtual async Task<bool> RoleExistsAsync(string roleId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(roleId, roleEntityName);

        return await Roles.AnyAsync(r => r.Id == roleId);
    }

    public virtual async Task<bool> RoleExistsByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(IdentityRole.Name));

        return await Roles.AnyAsync(r => r.Name == name);
    }


    public async Task<IdentityRole?> GetByIdAsync(string roleId)
        => await Roles.SingleOrDefaultAsync(u => u.Id == roleId);

    public async Task<IdentityRole?> GetByNameAsync(string name)
        => await Roles.SingleOrDefaultAsync(u => u.Name == name);
}
