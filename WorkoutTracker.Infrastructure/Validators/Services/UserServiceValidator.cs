using Microsoft.Extensions.Logging;
using System.Security.Claims;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Extensions;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Services;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Infrastructure.Validators.Models;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services;

public class UserServiceValidator
{
    readonly IUserRepository userRepository;
    readonly UserValidator userValidator;
    readonly UserDetailsValidator userDetailsValidator;
    readonly RoleValidator roleValidator;

    public UserServiceValidator(
        IUserRepository userRepository,
        UserValidator userValidator,
        UserDetailsValidator userDetailsValidator,
        RoleValidator roleValidator)
    {
        this.userRepository = userRepository;
        this.userValidator = userValidator;
        this.userDetailsValidator = userDetailsValidator;
        this.roleValidator = roleValidator;
    }

    #region CRUD

    public async Task ValidateAddAsync(User user)
    {
        await userValidator.ValidateForAddAsync(user);
    }

    public async Task ValidateCreateAsync(User user, string password)
    {
        await userValidator.ValidateForAddAsync(user);
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(password, nameof(password));
    }

    public async Task ValidateUpdateAsync(User user)
    {
        await userValidator.ValidateForEditAsync(user);
    }

    public async Task ValidateDeleteAsync(string userId)
    {
        await userValidator.EnsureExistsAsync(userId);
    }

    public Task ValidateGetByIdAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        return Task.CompletedTask;
    }

    public Task ValidateGetByUsernameAsync(string username)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(username, nameof(User.UserName));
        return Task.CompletedTask;
    }

    public Task ValidateGetByEmailAsync(string email)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(email, nameof(User.Email));
        return Task.CompletedTask;
    }

    public async Task ValidateGetByClaimsAsync(ClaimsPrincipal claims)
    {
        await ValidateGetByUsernameAsync(claims.Identity?.Name!);
    }

    public Task ValidateGetAllAsync()
    {
        return Task.CompletedTask;
    }

    #endregion

    #region User Details

    public async Task ValidateGetUserDetailsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateAddUserDetails(string userId, UserDetails userDetails)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);

        await userDetailsValidator.ValidateForAddAsync(userDetails);
    }

    public async Task ValidateUpdateUserDetailsAsync(string userId, UserDetails userDetails)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);

        await userDetailsValidator.ValidateForAddAsync(userDetails);

        var _userDetails = await userRepository.GetUserDetailsFromUserAsync(userId);

        if (_userDetails is not null && _userDetails.UserId != userId)
            throw UnauthorizedException.HaveNoPermissionToAction("update", "user details");
    }

    #endregion

    #region User Models

    public async Task ValidateGetUserMuscleSizesAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetUserBodyWeightsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetUserWorkoutsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetUserWorkoutRecordsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetUserCreatedExercisesAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateGetUserEquipmentsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);
    }

    #endregion

    #region Password

    public async Task ValidateChangePasswordAsync(string userId, string oldPassword, string newPassword)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(oldPassword, "Old password");
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(newPassword, "New password");

        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateAddPasswordAsync(string userId, string newPassword)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(newPassword, "New password");

        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateHasPasswordAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

        await userValidator.EnsureExistsAsync(userId);
    }

    #endregion

    #region Roles

    public async Task ValidateGetUserRolesAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));

        await userValidator.EnsureExistsAsync(userId);
    }

    public async Task ValidateAddRolesToUserAsync(string userId, string[] roles)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        ArgumentValidator.ThrowIfCollectionNullOrEmpty(roles, nameof(roles));

        await userValidator.EnsureExistsAsync(userId);

        foreach (var role in roles)
        {
            ArgumentValidator.ThrowIfArgumentNullOrEmpty(role, nameof(role));
            await roleValidator.EnsureExistsAsync(role);
        }
    }

    public async Task ValidateDeleteRoleFromUserAsync(string userId, string role)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(role, nameof(role));

        await userValidator.EnsureExistsAsync(userId);
        await roleValidator.EnsureExistsAsync(role);
    }

    public async Task ValidateGetUsersByRoleAsync(string role)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(role, "Role");
        await roleValidator.EnsureExistsAsync(role);
    }

    #endregion
}
