using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Infrastructure.Validators.Models;

namespace WorkoutTracker.Infrastructure.Validators.Services.Auth;

public class RoleServiceValidator
{
    readonly RoleValidator roleValidator;
    public RoleServiceValidator(RoleValidator roleValidator)
        => this.roleValidator = roleValidator;

    public async Task ValidateAddAsync(IdentityRole role)
    {
        await roleValidator.ValidateForAddAsync(role);
    }

    public async Task ValidateUpdateAsync(IdentityRole role)
    {
        await roleValidator.ValidateForEditAsync(role);
    }

    public async Task ValidateDeleteAsync(string roleId)
    {
        await roleValidator.EnsureExistsAsync(roleId);
    }

    public Task ValidateGetByIdAsync(string roleId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(roleId, "Role");
        return Task.CompletedTask;
    }

    public Task ValidateGetByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(IdentityRole.Name));
        return Task.CompletedTask;
    }

    public Task ValidateGetAllAsync()
    {
        return Task.CompletedTask;
    }

    public Task ValidateGetRoleNameByIdAsync(string roleId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(roleId, "Role");
        return Task.CompletedTask;
    }

    public Task ValidateGetRoleIdByNameAsync(string name)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(IdentityRole.Name));
        return Task.CompletedTask;
    }
}
