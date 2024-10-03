using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Data;
using System.Security.Principal;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.MuscleSizeServices;
using WorkoutTrackerAPI.Services.RoleServices;
using WorkoutTrackerAPI.Services.UserServices;

namespace WorkoutTrackerAPI.Controllers;

[Authorize]
public class UsersController : APIController
{
    readonly IUserService userService;
    readonly IMapper mapper;
    readonly IHttpContextAccessor httpContextAccessor;
    public UsersController(IUserService userService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        this.userService = userService;
        this.httpContextAccessor = httpContextAccessor;
        this.mapper = mapper;
    }

    ActionResult UserIDIsNullOrEmpty()
        => BadRequest("User ID is null or empty.");
    ActionResult UserNameIsNullOrEmpty()
        => BadRequest("User name is null or empty.");
    ActionResult RoleNameIsNullOrEmpty()
        => BadRequest("Role name is null or empty.");
    ActionResult UserIsNull()
        => EntryIsNull("User");
    ActionResult UserNotFound()
        => EntryNotFound("User");


    #region User

    [HttpGet]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<ActionResult<ApiResult<UserDTO>>> GetUsersAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        try
        {
            var users = await userService.GetUsersAsync();

            if (users is null)
                return EntryNotFound("Users");

            var userDTOs = mapper.Map<IQueryable<UserDTO>>(users);
            return await ApiResult<UserDTO>.CreateAsync(
                userDTOs,
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{userId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<ActionResult<UserDTO>> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        try
        {
            User? user = await userService.GetUserByIdAsync(userId);

            if (user is null)
                return UserNotFound();

            return mapper.Map<UserDTO>(user);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("current-user")]
    public async Task<ActionResult<UserDTO>> GetCurrentUserAsync()
    {
        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            User currentUser = (await userService.GetUserByIdAsync(userId))!;
            return mapper.Map<UserDTO>(currentUser);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpGet("userid-by-name/{username}")]
    public async Task<ActionResult<string>> GetUserIdByUserNameAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
            return UserNameIsNullOrEmpty();

        try
        {
            string? userId = await userService.GetUserIdByUsernameAsync(username);

            if (userId is null)
                return UserNotFound();

            return userId;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpGet("username")]
    public async Task<ActionResult<User>> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
            return UserNameIsNullOrEmpty();

        try
        {
            User? user = await userService.GetUserByUsernameAsync(username);

            if (user is null)
                return UserNotFound();

            return user;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpPost]
    public async Task<ActionResult<User>> AddUserAsync(UserCreationDTO user)
    {
        if (user is null)
            return UserIsNull();

        try
        {
            User _user = mapper.Map<User>(user);
            return await userService.AddUserAsync(_user);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUserAsync(string userId, User user)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        if (user is null)
            return UserIsNull();

        if (userId != user.Id)
            return EntryIDsNotMatch("User");

        try
        {
            string currentUserId = httpContextAccessor.GetUserId()!;
            var roles = await userService.GetUserRolesAsync(currentUserId);
            bool isAdmin = roles.Contains(Roles.AdminRole);

            bool isCurrentUserUpdating = user.Id == currentUserId;

            if (isAdmin || isCurrentUserUpdating)
            {
                var identityResult = await userService.UpdateUserAsync(user);
                return HandleIdentityResult(identityResult);
            }

            return Forbid("Access denied!");
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        try
        {
            var identityResult = await userService.DeleteUserAsync(userId);
            return HandleIdentityResult(identityResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("user-exists/{userId}")]
    public async Task<ActionResult<bool>> UserExistsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        try
        {
            return await userService.UserExistsAsync(userId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("user-exists-by-username/{userName}")]
    public async Task<ActionResult<bool>> UserExistsByUsernameAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return UserNameIsNullOrEmpty();

        try
        {
            return await userService.UserExistsByUsernameAsync(userName);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    #endregion

    #region UserModels

    [HttpGet($"user-muscle_sizes")]
    public async Task<ActionResult<ApiResult<MuscleSize>>> GetCurrentUserMuscleSizesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        try
        {
            string currentUserId = httpContextAccessor.GetUserId()!;
            var userMuscleSizes = await userService.GetUserMuscleSizesAsync(currentUserId);

            if (userMuscleSizes is null)
                return EntryNotFound("User Muscle Sizes");

            return await ApiResult<MuscleSize>.CreateAsync(
                userMuscleSizes.AsQueryable(),
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet($"user-body_weights")]
    public async Task<ActionResult<ApiResult<BodyWeight>>> GetCurrentUserBodyWeightsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        try
        {
            string currentUserId = httpContextAccessor.GetUserId()!;
            var userBodyWeights = await userService.GetUserBodyWeightsAsync(currentUserId);

            if (userBodyWeights is null)
                return EntryNotFound("User Body Weights");

            return await ApiResult<BodyWeight>.CreateAsync(
                userBodyWeights.AsQueryable(),
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet($"user-workouts")]
    public async Task<ActionResult<ApiResult<Workout>>> GetCurrentUserWorkoutsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        try
        {
            string currentUserId = httpContextAccessor.GetUserId()!;
            var userWorkouts = await userService.GetUserWorkoutsAsync(currentUserId);

            if (userWorkouts is null)
                return EntryNotFound("User Workouts");

            return await ApiResult<Workout>.CreateAsync(
                userWorkouts.AsQueryable(),
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet($"user-exercises")]
    public async Task<ActionResult<ApiResult<Exercise>>> GetCurrentUserCreatedExercisesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        try
        {
            string currentUserId = httpContextAccessor.GetUserId()!;
            var userCreatedExercises = await userService.GetUserCreatedExercisesAsync(currentUserId);

            if (userCreatedExercises is null)
                return EntryNotFound("User Created Exercises");

            return await ApiResult<Exercise>.CreateAsync(
                userCreatedExercises.AsQueryable(),
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet($"user-exercise_records")]
    public async Task<ActionResult<ApiResult<ExerciseRecord>>> GetCurrentUserExerciseRecordsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        try
        {
            string currentUserId = httpContextAccessor.GetUserId()!;
            var userExerciseRecords = await userService.GetUserExerciseRecordsAsync(currentUserId);

            if (userExerciseRecords is null)
                return EntryNotFound("User Exercise Records");

            return await ApiResult<ExerciseRecord>.CreateAsync(
                userExerciseRecords.AsQueryable(),
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    #endregion

    #region Password

    //[Authorize(Roles = Roles.AdminRole)]
    //[HttpGet("{userId}/has-password")]
    //public async Task<ActionResult<bool>> HasPassword(string userId)
    //{
    //    if (string.IsNullOrEmpty(userId))
    //        return UserIDIsNullOrEmpty();

    //    try
    //    {
    //        return await userService.HasUserPasswordAsync(userId);
    //    }
    //    catch (Exception ex)
    //    {
    //        return HandleException(ex);
    //    }
    //}

    [HttpPut("password")]
    public async Task<IActionResult> ChangeCurrentUserPasswordAsync(PasswordModel passwordModel)
    {
        if (passwordModel is null)
            return EntryIsNull("Password");

        if (!ModelState.IsValid)
            return HandleInvalidModelState();

        string currentUserId = httpContextAccessor.GetUserId()!;
        var identityResult = await userService.ChangeUserPasswordAsync(currentUserId, passwordModel.OldPassword!, passwordModel.NewPassword);
        return HandleIdentityResult(identityResult);
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpPost("password")]
    public async Task<IActionResult> CreatePassword(string userId, string newPassword)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        if (newPassword is null)
            return BadRequest("Password is null or empty.");

        var identityResult = await userService.AddUserPasswordAsync(userId, newPassword);
        return HandleIdentityResult(identityResult);
    }

    #endregion

    #region Roles

    [HttpGet("user-roles")]
    public async Task<ActionResult<IEnumerable<string>>> GetCurrentUserRolesAsync()
    {
        try
        {
            string currentUserId = httpContextAccessor.GetUserId()!;
            return (ActionResult)(await userService.GetUserRolesAsync(currentUserId));
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpPost("{userId}/role")]
    public async Task<IActionResult> AddRolesToUserAsync(string userId, string[] roles)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        if (roles.Length == 0)
            return BadRequest("User cannot have no roles.");

        var identityResult = await userService.AddRolesToUserAsync(userId, roles);
        return HandleIdentityResult(identityResult);
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpDelete("{userId}/{roleName}")]
    public async Task<IActionResult> DeleteRoleFromUserAsync(string userId, string roleName)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        if (string.IsNullOrEmpty(roleName))
            return RoleNameIsNullOrEmpty();

        var identityResult = await userService.DeleteRoleFromUserAsync(userId, roleName);
        return HandleIdentityResult(identityResult);
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpGet("users-by-role/{roleName}")]
    public async Task<ActionResult<ApiResult<UserDTO>>> GetUsersByRoleAsync(
        string roleName,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        if (string.IsNullOrEmpty(roleName))
            return RoleNameIsNullOrEmpty();

        try
        {
            var users = await userService.GetUsersByRoleAsync(roleName);
            var userDTOs = mapper.Map<IQueryable<UserDTO>>(users);

            return await ApiResult<UserDTO>.CreateAsync(
                userDTOs,
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    #endregion
}
