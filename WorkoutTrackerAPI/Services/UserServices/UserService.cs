using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.UserServices;
namespace WorkoutTrackerAPI.Services;

public class UserService : BaseService<User>, IUserService
{
    readonly RoleRepository roleRepository;
    readonly UserRepository userRepository;
    public UserService(UserRepository userRepository, RoleRepository roleRepository)
    {
        this.userRepository = userRepository;
        this.roleRepository = roleRepository;
    }

    readonly EntryNullException userIsNullException = new EntryNullException(nameof(User));
    readonly ArgumentNullOrEmptyException userNameIsNullOrEmptyException = new("User name");

    ArgumentException invalidUserIDWhileAdding => InvalidEntryIDWhileAdding(nameof(User), "user");


    #region CRUD

    public async Task<User> AddUserAsync(User user)
    {
        if (user is null)
            throw new EntryNullException(nameof(User));

        if (await UserExistsAsync(user.Id))
            throw new Exception("User already exists.");

        return await userRepository.AddUserAsync(user);
    }

    public async Task<IdentityResult> CreateUserAsync(User user, string password)
    {
        if (user is null)
            return IdentityResultExtentions.Failed(new EntryNullException(nameof(User)));

        if (string.IsNullOrEmpty(password))
            return IdentityResultExtentions.Failed(new ArgumentNullOrEmptyException("Password"));

        try
        {
            return await userRepository.CreateUserAsync(user, password);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToAction("user", "create", ex.Message));
        }
    }

    public async Task<IdentityResult> UpdateUserAsync(User user)
    {
        if (user is null)
            return IdentityResultExtentions.Failed(userIsNullException);

        if (string.IsNullOrEmpty(user.Id))
            return IdentityResultExtentions.Failed(userIdIsNullOrEmptyException);

        try
        {
            if (await UserDoesNotExist(user.Id))
                return IdentityResultExtentions.Failed(userNotFoundException);

            return await userRepository.UpdateUserAsync(user);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToAction("user", "update", ex.Message));
        }
    }

    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return IdentityResultExtentions.Failed(userIdIsNullOrEmptyException);

        try
        {
            if (await UserDoesNotExist(userId))
                return IdentityResultExtentions.Failed(userNotFoundException);

            return await userRepository.DeleteUserAsync(userId);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToAction("user", "delete", ex.Message));
        }
    }

    public async Task<User?> GetUserByClaimsAsync(ClaimsPrincipal claims)
    {
        if (claims is null)
            throw new EntryNullException("Claims");

        return await GetUserByUsernameAsync(claims.Identity?.Name!);
    }

    public async Task<string?> GetUserIdByUsernameAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            throw userNameIsNullOrEmptyException;

        var userByUsername = await userRepository.GetUserByUsernameAsync(userName);
        return userByUsername?.Id;
    }

    public async Task<string?> GetUserNameByIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        var userById = await userRepository.GetUserByIdAsync(userId);
        return userById?.UserName;
    }

    public async Task<User?> GetUserByUsernameAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            throw userNameIsNullOrEmptyException;

        return await userRepository.GetUserByUsernameAsync(userName);
    }

    public async Task<IQueryable<User>> GetUsersAsync()
        => await userRepository.GetUsersAsync();

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        return await userRepository.GetUserByIdAsync(userId);
    }

    public async Task<bool> UserExistsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        return await userRepository.UserExistsAsync(userId);
    }

    public async Task<bool> UserExistsByUsernameAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            throw userNameIsNullOrEmptyException;

        return await userRepository.UserExistsByUsernameAsync(userName);
    }

    #endregion

    #region User Models

    public async Task<IQueryable<ExerciseRecord>?> GetUserExerciseRecordsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        if (await UserDoesNotExist(userId))
            throw userNotFoundException;

        return await userRepository.GetUserExerciseRecordsAsync(userId);
    }

    public async Task<IQueryable<MuscleSize>?> GetUserMuscleSizesAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        if (await UserDoesNotExist(userId))
            throw userNotFoundException;

        return await userRepository.GetUserMuscleSizesAsync(userId);
    }

    public async Task<IQueryable<BodyWeight>?> GetUserBodyWeightsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        if (await UserDoesNotExist(userId))
            throw userNotFoundException;

        return await userRepository.GetUserBodyWeightsAsync(userId);
    }

    public async Task<IQueryable<Workout>?> GetUserWorkoutsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        if (await UserDoesNotExist(userId))
            throw userNotFoundException;

        return await userRepository.GetUserWorkoutsAsync(userId);
    }

    public async Task<IQueryable<Exercise>?> GetUserCreatedExercisesAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        if (await UserDoesNotExist(userId))
            throw userNotFoundException;

        return await userRepository.GetUserCreatedExercisesAsync(userId);
    }

    #endregion

    #region Password

    public async Task<IdentityResult> ChangeUserPasswordAsync(string userId, string oldPassword, string newPassword)
    {
        if (string.IsNullOrEmpty(userId))
            return IdentityResultExtentions.Failed(userIdIsNullOrEmptyException);

        if (await UserDoesNotExist(userId))
            return IdentityResultExtentions.Failed(userNotFoundException);

        if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
            return IdentityResultExtentions.Failed(new ArgumentNullOrEmptyException("Old or new password"));

        if (oldPassword == newPassword)
            return IdentityResultExtentions.Failed(new ArgumentException("The old password cannot be equal to the new one."));

        try
        {
            return await userRepository.ChangeUserPasswordAsync(userId, oldPassword, newPassword);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToAction("user password", "change", ex.Message));
        }
    }

    public async Task<IdentityResult> AddUserPasswordAsync(string userId, string newPassword)
    {
        if (string.IsNullOrEmpty(userId))
            return IdentityResultExtentions.Failed(userIdIsNullOrEmptyException);

        if (await UserDoesNotExist(userId))
            return IdentityResultExtentions.Failed(userNotFoundException);

        if (string.IsNullOrEmpty(newPassword))
            return IdentityResultExtentions.Failed(new ArgumentNullOrEmptyException("Password"));
        try
        {
            return await userRepository.AddUserPasswordAsync(userId, newPassword);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToAction("user password", "add", ex.Message));
        }
    }

    public async Task<bool> HasUserPasswordAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        if (await UserDoesNotExist(userId))
            throw userNotFoundException;

        return await userRepository.HasUserPasswordAsync(userId);
    }

    #endregion

    #region Roles

    readonly ArgumentNullOrEmptyException roleNameIsNullOrEmptyException = new("Role name");

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw userIdIsNullOrEmptyException;

        if (await UserDoesNotExist(userId))
            throw userNotFoundException;

        return await userRepository.GetUserRolesAsync(userId);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            throw roleNameIsNullOrEmptyException;

        if (await RoleDoesNotExistByName(roleName))
            throw new NotFoundException("Role");

        var allUser = await userRepository.GetUsersAsync();

        List<User> usersByRole = new();
        foreach (User user in allUser)
        {
            var userRoles = (await userRepository.GetUserRolesAsync(user.Id))!;
            if (userRoles.Contains(roleName))
                usersByRole.Add(user);
        }

        return usersByRole;
    }

    public async Task<IdentityResult> AddRolesToUserAsync(string userId, string[] roles)
    {
        if (string.IsNullOrEmpty(userId))
            return IdentityResultExtentions.Failed(userIdIsNullOrEmptyException);

        if (await UserDoesNotExist(userId))
            return IdentityResultExtentions.Failed(userNotFoundException);

        if (roles.Length == 0)
            return IdentityResultExtentions.Failed("User cannot have no roles.");

        foreach (var role in roles)
        {
            if (await RoleDoesNotExistByName(role))
                return IdentityResultExtentions.Failed(new NotFoundException($"Role '{role}'"));
        }

        try
        {
            return await userRepository.AddRolesToUserAsync(userId, roles);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToAction("roles to user", "add", ex.Message));
        }
    }

    public async Task<IdentityResult> DeleteRoleFromUserAsync(string userId, string roleName)
    {
        if (string.IsNullOrEmpty(userId))
            return IdentityResultExtentions.Failed(userIdIsNullOrEmptyException);

        if (await UserDoesNotExist(userId))
            return IdentityResultExtentions.Failed(userNotFoundException);

        if (string.IsNullOrEmpty(roleName))
            return IdentityResultExtentions.Failed(roleNameIsNullOrEmptyException);

        if (await RoleDoesNotExistByName(roleName))
            return IdentityResultExtentions.Failed(new NotFoundException("Role"));

        try
        {
            var userRoles = await userRepository.GetUserRolesAsync(userId);
            if (!userRoles.Contains(roleName))
                return IdentityResultExtentions.Failed($"User does not have '{roleName}' role");

            return await userRepository.DeleteRoleFromUserAsync(userId, roleName);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToAction("role from user", "delete", ex.Message));
        }
    }

    #endregion

    async Task<bool> UserDoesNotExist(string userId)
        => !(await userRepository.UserExistsAsync(userId));
    async Task<bool> UserDoesNotExistByName(string userName)
        => !(await userRepository.UserExistsByUsernameAsync(userName));

    async Task<bool> RoleDoesNotExist(string roleId)
        => !(await roleRepository.RoleExistsAsync(roleId));
    async Task<bool> RoleDoesNotExistByName(string roleName)
        => !(await roleRepository.RoleExistsByNameAsync(roleName));
}
