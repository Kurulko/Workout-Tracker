using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Extensions;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Services;
using WorkoutTracker.Infrastructure.Services.Base;

namespace WorkoutTracker.Infrastructure.Services.Auth;

internal class RoleService : BaseService<RoleService, User>, IRoleService
{
    readonly IRoleRepository roleRepository;
    public RoleService(IRoleRepository roleRepository, ILogger<RoleService> logger) : base(logger)
        => this.roleRepository = roleRepository;

    readonly EntryNullException roleIsNullException = new EntryNullException("Role");
    readonly ArgumentNullOrEmptyException roleNameIsNullOrEmptyException = new("Role name");
    readonly ArgumentNullOrEmptyException roleIdIsNullOrEmptyException = new("Role ID");

    NotFoundException RoleNotFoundByIDException(string id)
        => NotFoundException.NotFoundExceptionByID("Role", id);

    public async Task<IdentityRole> AddRoleAsync(IdentityRole role)
    {
        if (role is null)
            throw roleIsNullException;

        if (await RoleExistsAsync(role.Id) || await RoleExistsByNameAsync(role.Name!))
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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return IdentityResultExtensions.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("role", "delete"));
            throw;
        }
    }

    public async Task<IQueryable<IdentityRole>> GetRolesAsync()
        => await roleRepository.GetRolesAsync();

    public async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            throw roleIdIsNullOrEmptyException;

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
        catch (Exception ex) when (ex is IWorkoutException)
        {
            return IdentityResultExtensions.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, FailedToActionStr("role", "update"));
            throw;
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
            throw roleIdIsNullOrEmptyException;

        var roleById = await roleRepository.GetRoleByIdAsync(roleId);
        return roleById?.Name;
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

    async Task CheckRoleIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            throw roleIdIsNullOrEmptyException;

        bool roleExists = await roleRepository.RoleExistsAsync(roleId);
        if (!roleExists)
            throw RoleNotFoundByIDException(roleId);
    }
}
