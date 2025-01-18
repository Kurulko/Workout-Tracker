using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;

namespace WorkoutTrackerAPI.Services.RoleServices;

public class RoleService : BaseService<User>, IRoleService
{
    readonly RoleRepository roleRepository;
    public RoleService(RoleRepository roleRepository)
        => this.roleRepository = roleRepository;

    readonly EntryNullException roleIsNullException = new EntryNullException("Role");
    readonly ArgumentNullOrEmptyException roleNameIsNullOrEmptyException = new("Role name");
    readonly ArgumentNullOrEmptyException RoleIdIsNullOrEmptyException = new("Role ID");

    NotFoundException RoleNotFoundByIDException(string id)
        => NotFoundException.NotFoundExceptionByID("Role", id);

    public async Task<IdentityRole> AddRoleAsync(IdentityRole role)
    {
        if (role is null)
            throw roleIsNullException;

        if (await RoleExistsAsync(role.Id) || await RoleExistsByNameAsync(role.Name))
            throw new Exception("Role already exists.");

        return await roleRepository.AddRoleAsync(role);
    }

    public async Task<IdentityResult> DeleteRoleAsync(string roleId)
    {
        try
        {
            await CheckRoleIdAsync(roleId);

            return await roleRepository.DeleteRoleAsync(roleId);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtentions.Failed(ex);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToActionStr("role", "delete", ex));
        }
    }

    public async Task<IQueryable<IdentityRole>> GetRolesAsync()
        => await roleRepository.GetRolesAsync();

    public async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            throw RoleIdIsNullOrEmptyException;

        return await roleRepository.GetRoleByIdAsync(roleId);
    }

    public async Task<IdentityRole?> GetRoleByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw roleNameIsNullOrEmptyException;

        return await roleRepository.GetRoleByNameAsync(name);
    }

    public async Task<IdentityResult> UpdateRoleAsync(IdentityRole role)
    {
        try
        {
            if (role is null)
                throw roleIsNullException;

            await CheckRoleIdAsync(role.Id);

            return await roleRepository.UpdateRoleAsync(role);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtentions.Failed(ex);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToActionStr("role", "update", ex));
        }
    }

    public async Task<string?> GetRoleIdByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw roleNameIsNullOrEmptyException;

        var roleByName = await roleRepository.GetRoleByNameAsync(name);
        return roleByName?.Id;
    }

    public async Task<string?> GetRoleNameByIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            throw RoleIdIsNullOrEmptyException;

        var roleById = await roleRepository.GetRoleByIdAsync(roleId);
        return roleById?.Name;
    }

    public async Task<bool> RoleExistsAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            throw RoleIdIsNullOrEmptyException;

        return await roleRepository.RoleExistsAsync(roleId);
    }

    public async Task<bool> RoleExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw roleNameIsNullOrEmptyException;

        return await roleRepository.RoleExistsByNameAsync(name);
    }

    async Task CheckRoleIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            throw RoleIdIsNullOrEmptyException;

        bool roleExists = await roleRepository.RoleExistsAsync(roleId);
        if (!roleExists)
            throw RoleNotFoundByIDException(roleId);
    }
}
