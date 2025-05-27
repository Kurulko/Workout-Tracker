using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Services;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Infrastructure.Validators.Services;

namespace WorkoutTracker.Infrastructure.Services;

internal class UserService : BaseService<UserService, User>, IUserService
{
    readonly IUserRepository userRepository;
    readonly IUserDetailsRepository userDetailsRepository;
    readonly UserServiceValidator userServiceValidator;

    public UserService(
        IUserRepository userRepository,
        IUserDetailsRepository userDetailsRepository,
        UserServiceValidator userServiceValidator,
        ILogger<UserService> logger
    ) : base(logger)
    {
        this.userRepository = userRepository;
        this.userDetailsRepository = userDetailsRepository;
        this.userServiceValidator = userServiceValidator;
    }


    #region CRUD

    const string userEntityName = "user";

    public async Task<User> AddUserAsync(User user)
    {
        await userServiceValidator.ValidateAddAsync(user);

        return await userRepository.AddUserAsync(user)
            .LogExceptionsAsync(_logger, FailedToActionStr(userEntityName, "add"));
    }

    public async Task CreateUserAsync(User user, string password)
    {
        await userServiceValidator.ValidateCreateAsync(user, password);

        await userRepository.CreateUserAsync(user, password)
            .LogExceptionsAsync(_logger, FailedToActionStr(userEntityName, "create"));
    }

    public async Task UpdateUserAsync(User user)
    {
        await userServiceValidator.ValidateUpdateAsync(user);

        await userRepository.UpdateUserAsync(user)
            .LogExceptionsAsync(_logger, FailedToActionStr(userEntityName, "update"));
    }

    public async Task DeleteUserAsync(string userId)
    {
        await userServiceValidator.ValidateDeleteAsync(userId);

        await userRepository.DeleteUserAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionStr(userEntityName, "delete"));
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        await userServiceValidator.ValidateGetByIdAsync(userId);

        return await userRepository.GetUserByIdAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionStr(userEntityName, "get"));
    }

    public async Task<User?> GetUserByUsernameAsync(string userName)
    {
        await userServiceValidator.ValidateGetByUsernameAsync(userName);

        return await userRepository.GetUserByUsernameAsync(userName)
            .LogExceptionsAsync(_logger, FailedToActionStr(userEntityName, "get"));
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        await userServiceValidator.ValidateGetByEmailAsync(email);

        return await userRepository.GetUserByEmailAsync(email)
            .LogExceptionsAsync(_logger, FailedToActionStr(userEntityName, "get"));
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        await userServiceValidator.ValidateGetAllAsync();

        var users = userRepository.GetUsers();

        return await users.ToListAsync()
            .LogExceptionsAsync(_logger, FailedToActionStr("users", "get"));
    }

    public async Task<User?> GetUserByClaimsAsync(ClaimsPrincipal claims)
    {
        await userServiceValidator.ValidateGetByClaimsAsync(claims);

        return await GetUserByUsernameAsync(claims.Identity?.Name!);
    }

    public async Task<string?> GetUserIdByUsernameAsync(string userName)
    {
        await userServiceValidator.ValidateGetByUsernameAsync(userName);

        return await userRepository.GetUserIdByUsernameAsync(userName)
            .LogExceptionsAsync(_logger, FailedToActionStr(userEntityName, "get"));
    }

    public async Task<string?> GetUserNameByIdAsync(string userId)
    {
        await userServiceValidator.ValidateGetByIdAsync(userId);

        return await userRepository.GetUserNameByIdAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionStr(userEntityName, "get"));
    }


    #endregion

    #region User Details

    const string userDetailsEntityName = "user details";

    public async Task<UserDetails?> GetUserDetailsFromUserAsync(string userId)
    {
        await userServiceValidator.ValidateGetUserDetailsAsync(userId);

        return await userRepository.GetUserDetailsFromUserAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionStr(userDetailsEntityName, "get"));
    }

    public async Task AddUserDetailsToUserAsync(string userId, UserDetails userDetails, CancellationToken cancellationToken)
    {
        await userServiceValidator.ValidateAddUserDetails(userId, userDetails, cancellationToken);

        userDetails.UserId = userId;

        await userDetailsRepository.AddAsync(userDetails)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userDetailsEntityName, "add", userId));
    }

    public async Task UpdateUserDetailsFromUserAsync(string userId, UserDetails userDetails, CancellationToken cancellationToken)
    {
        await userServiceValidator.ValidateUpdateUserDetailsAsync(userId, userDetails, cancellationToken);

        var _userDetails = await userRepository.GetUserDetailsFromUserAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(userDetailsEntityName, "get", userId));

        if (_userDetails is null)
        {
            userDetails.UserId = userId;

            await userDetailsRepository.AddAsync(userDetails)
                .LogExceptionsAsync(_logger, FailedToActionForUserStr(userDetailsEntityName, "add", userId));
        }
        else
        {

            var updateAction = new Action<UserDetails>(ud =>
            {
                ud.Gender = userDetails.Gender;
                ud.Weight = userDetails.Weight;
                ud.Height = userDetails.Height;
                ud.DateOfBirth = userDetails.DateOfBirth;
                ud.BodyFatPercentage = userDetails.BodyFatPercentage;
            });

            await userDetailsRepository.UpdatePartialAsync(userDetails.Id, updateAction)
                .LogExceptionsAsync(_logger, FailedToActionForUserStr(userDetailsEntityName, "update", userId));
        }
    }

    #endregion

    #region User Models

    public async Task<IEnumerable<MuscleSize>?> GetUserMuscleSizesAsync(string userId)
    {
        await userServiceValidator.ValidateGetUserMuscleSizesAsync(userId);

        return (await userRepository.GetUserMuscleSizesAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("muscle sizes", "get", userId)))
            ?.ToList();
    }

    public async Task<IEnumerable<BodyWeight>?> GetUserBodyWeightsAsync(string userId)
    {
        await userServiceValidator.ValidateGetUserBodyWeightsAsync(userId);

        return (await userRepository.GetUserBodyWeightsAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("body weights", "get", userId)))
            ?.ToList();
    }

    public async Task<IEnumerable<Workout>?> GetUserWorkoutsAsync(string userId)
    {
        await userServiceValidator.ValidateGetUserWorkoutsAsync(userId);

        return (await userRepository.GetUserWorkoutsAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("workouts", "get", userId)))
            ?.ToList();
    }

    public async Task<IEnumerable<WorkoutRecord>?> GetUserWorkoutRecordsAsync(string userId)
    {
        await userServiceValidator.ValidateGetUserWorkoutRecordsAsync(userId);

        return (await userRepository.GetUserWorkoutRecordsAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("workout records", "get", userId)))
            ?.ToList();
    }

    public async Task<IEnumerable<Exercise>?> GetUserCreatedExercisesAsync(string userId)
    {
        await userServiceValidator.ValidateGetUserCreatedExercisesAsync(userId);

        return (await userRepository.GetUserCreatedExercisesAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("created exercises", "get", userId)))
            ?.ToList();
    }

    public async Task<IEnumerable<Equipment>?> GetUserEquipmentsAsync(string userId)
    {
        await userServiceValidator.ValidateGetUserEquipmentsAsync(userId);

        return (await userRepository.GetUserEquipmentsAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("equipments", "get", userId)))
            ?.ToList();
    }

    #endregion

    #region Password

    const string passwordEntityName = "user password";

    public async Task ChangeUserPasswordAsync(string userId, string oldPassword, string newPassword)
    {
        await userServiceValidator.ValidateChangePasswordAsync(userId, oldPassword, newPassword);

        await userRepository.ChangeUserPasswordAsync(userId, oldPassword, newPassword)
            .LogExceptionsAsync(_logger, FailedToActionStr(passwordEntityName, "change"));
    }

    public async Task AddUserPasswordAsync(string userId, string newPassword)
    {
        await userServiceValidator.ValidateAddPasswordAsync(userId, newPassword);

        await userRepository.AddUserPasswordAsync(userId, newPassword)
            .LogExceptionsAsync(_logger, FailedToActionStr(passwordEntityName, "add"));
    }

    public async Task<bool> HasUserPasswordAsync(string userId)
    {
        await userServiceValidator.ValidateHasPasswordAsync(userId);

        return await userRepository.HasUserPasswordAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionStr(passwordEntityName, "check"));
    }

    #endregion

    #region Roles

    const string roleEntityName = "user role";

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        await userServiceValidator.ValidateGetUserRolesAsync(userId);

        return await userRepository.GetUserRolesAsync(userId)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("user roles", "get", userId));
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
    {
        await userServiceValidator.ValidateGetUsersByRoleAsync(roleName);

        return await userRepository.GetUsersByRoleAsync(roleName)
            .LogExceptionsAsync(_logger, FailedToActionStr("users", "get"));
    }

    public async Task AddRolesToUserAsync(string userId, string[] roles)
    {
        await userServiceValidator.ValidateAddRolesToUserAsync(userId, roles);

        await userRepository.AddRolesToUserAsync(userId, roles)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr("roles to user", "add", userId));
    }

    public async Task DeleteRoleFromUserAsync(string userId, string roleName)
    {
        await userServiceValidator.ValidateDeleteRoleFromUserAsync(userId, roleName);

        await userRepository.DeleteRoleFromUserAsync(userId, roleName)
            .LogExceptionsAsync(_logger, FailedToActionForUserStr(roleEntityName, "delete", userId));
    }

    #endregion
}
