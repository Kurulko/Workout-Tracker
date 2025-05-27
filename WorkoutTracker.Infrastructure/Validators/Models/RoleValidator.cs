using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models;

public class RoleValidator
{
    readonly IRoleRepository roleRepository;
    public RoleValidator(IRoleRepository roleRepository)
        => this.roleRepository = roleRepository;

    const string roleEntryStr = "Role";

    public async Task ValidateForAddAsync(IdentityRole model)
    {
        await EnsureNonExistsAsync(model.Id);
        await ArgumentValidator.EnsureNonExistsByNameAsync(roleRepository.GetRoleByNameAsync, model.Name!);
    }

    public async Task<IdentityRole> ValidateForEditAsync(IdentityRole model)
    {
        var role = await EnsureExistsAsync(model.Id);
        await EnsureNameUniqueAsync(model.Name!, model.Id);

        return role;
    }

    public async Task<IdentityRole> EnsureExistsAsync(string id)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(id, roleEntryStr);

        return await ArgumentValidator.EnsureExistsByIdAsync(roleRepository.GetRoleByIdAsync, id, roleEntryStr);
    }

    public async Task EnsureNonExistsAsync(string id)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(id, roleEntryStr);

        await ArgumentValidator.EnsureNonExistsByIdAsync(roleRepository.GetRoleByIdAsync, id);
    }

    async Task EnsureNameUniqueAsync(string name, string id)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(IdentityRole.Name));

        var role = await roleRepository.GetRoleByNameAsync(name);

        if (role != null && role.Id != id)
            throw new ValidationException($"{nameof(IdentityRole.Name)} must be unique."); ;
    }
}