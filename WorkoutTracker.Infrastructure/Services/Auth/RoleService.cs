using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Services;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Infrastructure.Validators.Services.Auth;

namespace WorkoutTracker.Infrastructure.Services.Auth;

internal class RoleService : BaseService<RoleService, User>, IRoleService
{
    readonly IRoleRepository roleRepository;
    readonly RoleServiceValidator roleServiceValidator;
    public RoleService(
        IRoleRepository roleRepository,
        RoleServiceValidator roleServiceValidator,
        ILogger<RoleService> logger
    ) : base(logger)
    {
        this.roleRepository = roleRepository;
        this.roleServiceValidator = roleServiceValidator;
    }

    const string roleEntityName = "role";


    public async Task<IdentityRole> AddRoleAsync(IdentityRole role)
    {
        await roleServiceValidator.ValidateAddAsync(role);

        return await roleRepository.AddRoleAsync(role)
            .LogExceptionsAsync(_logger, FailedToActionStr(roleEntityName, "add"));
    }

    public async Task DeleteRoleAsync(string roleId)
    {
        await roleServiceValidator.ValidateDeleteAsync(roleId);

        await roleRepository.DeleteRoleAsync(roleId)
            .LogExceptionsAsync(_logger, FailedToActionStr(roleEntityName, "delete"));
    }

    public async Task<IEnumerable<IdentityRole>> GetRolesAsync()
    {
        await roleServiceValidator.ValidateGetAllAsync();

        var roles = roleRepository.GetRoles();

        return await roles.ToListAsync()
            .LogExceptionsAsync(_logger, FailedToActionStr("roles", "get"));
    }

    public async Task<IdentityRole?> GetRoleByIdAsync(string roleId)
    {
        await roleServiceValidator.ValidateGetByIdAsync(roleId);

        return await roleRepository.GetRoleByIdAsync(roleId)
            .LogExceptionsAsync(_logger, FailedToActionStr(roleEntityName, "get"));
    }

    public async Task<IdentityRole?> GetRoleByNameAsync(string name)
    {
        await roleServiceValidator.ValidateGetByNameAsync(name);

        return await roleRepository.GetRoleByNameAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionStr(roleEntityName, "get"));
    }

    public async Task UpdateRoleAsync(IdentityRole role)
    {
        await roleServiceValidator.ValidateUpdateAsync(role);

        await roleRepository.UpdateRoleAsync(role)
            .LogExceptionsAsync(_logger, FailedToActionStr(roleEntityName, "update"));
    }

    public async Task<string?> GetRoleIdByNameAsync(string name)
    {
        await roleServiceValidator.ValidateGetByNameAsync(name);

        return await roleRepository.GetRoleIdByNameAsync(name)
            .LogExceptionsAsync(_logger, FailedToActionStr("role ID", "get"));
    }

    public async Task<string?> GetRoleNameByIdAsync(string roleId)
    {
        await roleServiceValidator.ValidateGetByIdAsync(roleId);

        return await roleRepository.GetRoleNameByIdAsync(roleId)
            .LogExceptionsAsync(_logger, FailedToActionStr("role name", "get"));
    }
}
