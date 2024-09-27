using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Repositories;

namespace WorkoutTrackerAPI.Services.RoleServices;

public class RoleService : BaseService<User>, IRoleService
{
    readonly RoleRepository roleRepository;
    public RoleService(RoleRepository roleRepository)
        => this.roleRepository = roleRepository;

    readonly EntryNullException roleIsNullException = new EntryNullException(nameof(Role));
    readonly NotFoundException roleNotFoundException = new NotFoundException(nameof(Role));
    readonly ArgumentNullOrEmptyException roleNameIsNullOrEmptyException = new("Role name");
    readonly ArgumentNullOrEmptyException roleIdIsNullOrEmptyException = new("Role ID");


    public async Task<Role> AddRoleAsync(Role role)
    {
        if (role is null)
            throw roleIsNullException;

        if (!string.IsNullOrEmpty(role.Id))
            throw new ArgumentException(InvalidEntryIDWhileAddingStr(nameof(Role), "role"));

        if (await RoleDoesNotExist(role.Id))
            throw roleNotFoundException;

        return await roleRepository.AddRoleAsync(role);
    }

    public async Task<IdentityResult> DeleteRoleAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            return IdentityResultExtentions.Failed(roleIdIsNullOrEmptyException);

        if (await RoleDoesNotExist(roleId))
            return IdentityResultExtentions.Failed(roleNotFoundException);

        try
        {
            Role? role = await roleRepository.GetRoleByIdAsync(roleId);

            if (role is null)
                return IdentityResultExtentions.Failed(roleNotFoundException);

            return await roleRepository.DeleteRoleAsync(roleId);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToAction("role", "delete", ex.Message));
        }
    }

    public async Task<IQueryable<Role>> GetRolesAsync()
        => await roleRepository.GetRolesAsync();

    public async Task<Role?> GetRoleByIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            throw roleIdIsNullOrEmptyException;

        return await roleRepository.GetRoleByIdAsync(roleId);
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw roleNameIsNullOrEmptyException;

        return await roleRepository.GetRoleByNameAsync(name);
    }

    public async Task<IdentityResult> UpdateRoleAsync(Role role)
    {
        if (role is null)
            return IdentityResultExtentions.Failed(roleIsNullException);

        if (string.IsNullOrEmpty(role.Id))
            return IdentityResultExtentions.Failed(roleIdIsNullOrEmptyException);

        if (await RoleDoesNotExist(role.Id))
            return IdentityResultExtentions.Failed(roleNotFoundException);

        try
        {
            return await roleRepository.UpdateRoleAsync(role);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToAction("role", "update", ex.Message));
        }
    }

    public async Task<string?> GetRoleIdByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw roleNameIsNullOrEmptyException;

        return await roleRepository.GetRoleIdByNameAsync(name);
    }

    public async Task<string?> GetRoleNameByIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            throw roleIdIsNullOrEmptyException;

        return await roleRepository.GetRoleNameByIdAsync(roleId);
    }

    public async Task<bool> RoleExistsAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            throw roleIdIsNullOrEmptyException;

        return await roleRepository.RoleExistsAsync(roleId);
    }

    public async Task<bool> RoleExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw roleNameIsNullOrEmptyException;

        return await roleRepository.RoleExistsByNameAsync(name);
    }

    async Task<bool> RoleDoesNotExist(string roleId)
        => !(await roleRepository.RoleExistsAsync(roleId));
    async Task<bool> RoleDoesNotExistByName(string roleName)
        => !(await roleRepository.RoleExistsByNameAsync(roleName));
}
