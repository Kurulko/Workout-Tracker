using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Extensions;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Services;
using WorkoutTracker.Infrastructure.Services.Base;

namespace WorkoutTracker.Infrastructure.Services;

internal class UserService : BaseService<User>, IUserService
{
    readonly IUserRepository userRepository;
    readonly IUserDetailsRepository userDetailsRepository;
    readonly IRoleRepository roleRepository;
    public UserService(
        IUserRepository userRepository, 
        IUserDetailsRepository userDetailsRepository, 
        IRoleRepository roleRepository)
    {
        this.userRepository = userRepository;
        this.userDetailsRepository = userDetailsRepository;
        this.roleRepository = roleRepository;
    }

    readonly EntryNullException userIsNullException = new EntryNullException(nameof(User));
    readonly ArgumentNullOrEmptyException userNameIsNullOrEmptyException = new("User name");

    NotFoundException RoleNotFoundByIDException(string id)
        => NotFoundException.NotFoundExceptionByID("Role", id);
    NotFoundException RoleNotFoundByNameException(string name)
        => NotFoundException.NotFoundExceptionByName("Role", name);

    ArgumentException InvalidUserIDWhileAdding => InvalidEntryIDWhileAddingException(nameof(User), "user");


    #region CRUD

    public async Task<User> AddUserAsync(User user)
    {
        if (user is null)
            throw userIsNullException;

        if (await UserExistsAsync(user.Id) || await UserExistsByUsernameAsync(user.UserName!))
            throw new Exception("User already exists.");

        return await userRepository.AddUserAsync(user);
    }

    public async Task<IdentityResult> CreateUserAsync(User user, string password)
    {
        if (user is null)
            return IdentityResultExtensions.Failed(new EntryNullException(nameof(User)));

        if (string.IsNullOrEmpty(password))
            return IdentityResultExtensions.Failed(new ArgumentNullOrEmptyException("Password"));

        try
        {
            return await userRepository.CreateUserAsync(user, password);
        }
        catch (Exception ex)
        {
            return IdentityResultExtensions.Failed(FailedToActionStr("user", "create", ex));
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
            return IdentityResultExtensions.Failed(ex);
        }
        catch (Exception ex)
        {
            return IdentityResultExtensions.Failed(FailedToActionStr("user", "update", ex));
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
            return IdentityResultExtensions.Failed(ex);
        }
        catch (Exception ex)
        {
            return IdentityResultExtensions.Failed(FailedToActionStr("user", "delete", ex));
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

    #region User Details

    readonly EntryNullException userDetailsIsNullException = new("User details");
    readonly InvalidIDException invalidUserDetailsIDException = new(nameof(UserDetails));
    readonly NotFoundException userDetailsNotFoundException = new("User details");

    public async Task<UserDetails?> GetUserDetailsFromUserAsync(string userId)
    {
        await CheckUserIdAsync(userId);
        return await userRepository.GetUserDetailsFromUserAsync(userId);
    }

    public async Task<ServiceResult> AddUserDetailsToUserAsync(string userId, UserDetails userDetails)
    {
        try
        {
            await CheckUserIdAsync(userId);

            if (userDetails is null)
                throw userDetailsIsNullException;

            if (userDetails.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(UserDetails), "user details");

            userDetails.UserId = userId;
            await userDetailsRepository.AddAsync(userDetails);

            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("user details", "add", ex));
        }
    }

    public async Task<ServiceResult> UpdateUserDetailsFromUserAsync(string userId, UserDetails userDetails)
    {
        try
        {
            await CheckUserIdAsync(userId);

            if (userDetails is null)
                throw userDetailsIsNullException;

            var _userDetails = await userRepository.GetUserDetailsFromUserAsync(userId);
            if (_userDetails is null)
            {
                userDetails.UserId = userId;
                await userDetailsRepository.AddAsync(userDetails);
            }
            else
            {
                if (_userDetails.UserId != userId)
                    throw UserNotHavePermissionException("update", "user details");

                _userDetails.Gender = userDetails.Gender;
                _userDetails.Weight = userDetails.Weight;
                _userDetails.Height = userDetails.Height;
                _userDetails.DateOfBirth = userDetails.DateOfBirth;
                _userDetails.BodyFatPercentage = userDetails.BodyFatPercentage;

                await userDetailsRepository.UpdateAsync(_userDetails);
            }
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("user details", "update", ex));
        }
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
            return IdentityResultExtensions.Failed(ex);
        }
        catch (Exception ex)
        {
            return IdentityResultExtensions.Failed(FailedToActionStr("user password", "change", ex));
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
            return IdentityResultExtensions.Failed(ex);
        }
        catch (Exception ex)
        {
            return IdentityResultExtensions.Failed(FailedToActionStr("user password", "add", ex));
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
                    throw RoleNotFoundByNameException(roleStr);
            }

            return await userRepository.AddRolesToUserAsync(userId, roles);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtensions.Failed(ex);
        }
        catch (Exception ex)
        {
            return IdentityResultExtensions.Failed(FailedToActionStr("roles to user", "add", ex));
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
                return IdentityResultExtensions.Failed($"User does not have '{roleName}' role");

            return await userRepository.DeleteRoleFromUserAsync(userId, roleName);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return IdentityResultExtensions.Failed(ex);
        }
        catch (Exception ex)
        {
            return IdentityResultExtensions.Failed(FailedToActionStr("role from user", "delete", ex));
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
            throw RoleNotFoundByNameException(roleName);
    }
}
