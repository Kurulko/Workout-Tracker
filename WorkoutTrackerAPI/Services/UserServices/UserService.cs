using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.IO;
using System.Security.Claims;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
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
    readonly NotFoundException roleNotFoundException = new("Role");

    ArgumentException InvalidUserIDWhileAdding => InvalidEntryIDWhileAddingException(nameof(User), "user");


    #region CRUD

    public async Task<User> AddUserAsync(User user)
    {
        if (user is null)
            throw userIsNullException;

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
            return IdentityResultExtentions.Failed(FailedToActionStr("user", "create", ex.Message));
        }
    }

    public async Task<IdentityResult> UpdateUserAsync(User user)
    {
        try
        {
            if (user is null)
                throw userIsNullException;

            await CheckUserIdAsync(user.Id);

            return await userRepository.UpdateUserAsync(user);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtentions.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToActionStr("user", "update", ex.Message));
        }
    }

    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userId);

            return await userRepository.DeleteUserAsync(userId);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtentions.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToActionStr("user", "delete", ex.Message));
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
        await CheckUserIdAsync(userId);
        return await userRepository.GetUserExerciseRecordsAsync(userId);
    }

    public async Task<IQueryable<MuscleSize>?> GetUserMuscleSizesAsync(string userId)
    {
        await CheckUserIdAsync(userId);
        return await userRepository.GetUserMuscleSizesAsync(userId);
    }

    public async Task<IQueryable<BodyWeight>?> GetUserBodyWeightsAsync(string userId)
    {
        await CheckUserIdAsync(userId);
        return await userRepository.GetUserBodyWeightsAsync(userId);
    }

    public async Task<IQueryable<Workout>?> GetUserWorkoutsAsync(string userId)
    {
        await CheckUserIdAsync(userId);
        return await userRepository.GetUserWorkoutsAsync(userId);
    }

    public async Task<IQueryable<Exercise>?> GetUserCreatedExercisesAsync(string userId)
    {
        await CheckUserIdAsync(userId);
        return await userRepository.GetUserCreatedExercisesAsync(userId);
    }

    public async Task<IQueryable<Equipment>?> GetUserEquipmentsAsync(string userId)
    {
        await CheckUserIdAsync(userId);
        return await userRepository.GetUserEquipmentsAsync(userId);
    }

    #endregion

    #region Password

    public async Task<IdentityResult> ChangeUserPasswordAsync(string userId, string oldPassword, string newPassword)
    {
        try
        {
            await CheckUserIdAsync(userId);

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
                throw new ArgumentNullOrEmptyException("Old or new password");

            if (oldPassword == newPassword)
                throw new ArgumentException("The old password cannot be equal to the new one.");

            return await userRepository.ChangeUserPasswordAsync(userId, oldPassword, newPassword);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtentions.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToActionStr("user password", "change", ex.Message));
        }
    }

    public async Task<IdentityResult> AddUserPasswordAsync(string userId, string newPassword)
    {
        try
        {
            await CheckUserIdAsync(userId);

            if (string.IsNullOrEmpty(newPassword))
                throw new ArgumentNullOrEmptyException("Password");

            return await userRepository.AddUserPasswordAsync(userId, newPassword);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtentions.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToActionStr("user password", "add", ex.Message));
        }
    }

    public async Task<bool> HasUserPasswordAsync(string userId)
    {
        await CheckUserIdAsync(userId);

        return await userRepository.HasUserPasswordAsync(userId);
    }

    #endregion

    #region Roles

    readonly ArgumentNullOrEmptyException roleNameIsNullOrEmptyException = new("Role name");

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        await CheckUserIdAsync(userId);

        return await userRepository.GetUserRolesAsync(userId);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
    {
        await CheckRoleNameAsync(roleName);

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
        try
        {
            await CheckUserIdAsync(userId);

            if (roles.Length == 0)
                throw new ArgumentException("User cannot have no roles.");

            foreach (var roleStr in roles)
            {
                var roleExists = await roleRepository.RoleExistsByNameAsync(roleStr);
                if (!roleExists)
                    throw new NotFoundException($"Role '{roleStr}'");
            }

            return await userRepository.AddRolesToUserAsync(userId, roles);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtentions.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToActionStr("roles to user", "add", ex.Message));
        }
    }

    public async Task<IdentityResult> DeleteRoleFromUserAsync(string userId, string roleName)
    {
        try
        {
            await CheckUserIdAsync(userId);
            await CheckRoleNameAsync(roleName);

            var userRoles = await userRepository.GetUserRolesAsync(userId);
            if (!userRoles.Contains(roleName))
                return IdentityResultExtentions.Failed($"User does not have '{roleName}' role");

            return await userRepository.DeleteRoleFromUserAsync(userId, roleName);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtentions.Failed(ex.Message);
        }
        catch (Exception ex)
        {
            return IdentityResultExtentions.Failed(FailedToActionStr("role from user", "delete", ex.Message));
        }
    }

    #endregion

    async Task CheckUserIdAsync(string userId)
        => await CheckUserIdAsync(userRepository, userId);

    async Task CheckRoleNameAsync(string roleName)
    {
        if (string.IsNullOrEmpty(roleName))
            throw roleNameIsNullOrEmptyException;

        bool roleExists = await roleRepository.RoleExistsByNameAsync(roleName);
        if (!roleExists)
            throw roleNotFoundException;
    }
}
