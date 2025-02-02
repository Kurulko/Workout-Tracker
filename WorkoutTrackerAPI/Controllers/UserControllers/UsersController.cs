﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Security.Principal;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.BodyWeightServices;
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


    #region CRUD

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

            var userDTOs = users.AsEnumerable().Select(u => mapper.Map<UserDTO>(u));
            return await ApiResult<UserDTO>.CreateAsync(
                userDTOs.AsQueryable(),
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
    [ActionName(nameof(GetUserByIdAsync))]
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
    public async Task<IActionResult> AddUserAsync(UserCreationDTO userCreationDTO)
    {
        if (userCreationDTO is null)
            return UserIsNull();

        try
        {
            var user = mapper.Map<User>(userCreationDTO);
            await userService.AddUserAsync(user);

            var userDTO = mapper.Map<UserDTO>(user);
            return CreatedAtAction(nameof(GetUserByIdAsync), new { userId = userDTO.UserId }, userDTO);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpPost("create")]
    public async Task<IActionResult> CreateUserAsync(UserCreationDTO userCreationDTO, string password)
    {
        if (userCreationDTO is null)
            return UserIsNull();

        if (string.IsNullOrEmpty(password))
            return BadRequest("Password is null or empty.");

        var user = mapper.Map<User>(userCreationDTO);
        var result = await userService.CreateUserAsync(user, password);

        if (!result.Succeeded)
            return HandleIdentityResult(result);

        var userDTO = mapper.Map<UserDTO>(user);
        return CreatedAtAction(nameof(GetUserByIdAsync), new { userId = userDTO.UserId }, userDTO);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUserAsync(string userId, UserDTO userDTO)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        if (userDTO is null)
            return UserIsNull();

        if (userId != userDTO.UserId)
            return EntryIDsNotMatch("User");

        try
        {
            string currentUserId = httpContextAccessor.GetUserId()!;
            var roles = await userService.GetUserRolesAsync(currentUserId);
            bool isAdmin = roles.Contains(Roles.AdminRole);

            bool isCurrentUserUpdating = userDTO.UserId == currentUserId;

            if (isAdmin || isCurrentUserUpdating)
            {
                var user = mapper.Map<User>(userDTO);
                var identityResult = await userService.UpdateUserAsync(user);
                return HandleIdentityResult(identityResult);
            }

            return Forbid();
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

    [HttpGet("id-by-name/{name}")]
    public async Task<ActionResult<string>> GetUserIdByUsernameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return UserNameIsNullOrEmpty();

        try
        {
            string? userId = await userService.GetUserIdByUsernameAsync(name);

            if (userId is null)
                return UserNotFound();

            return userId;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    [HttpGet("name-by-id/{userId}")]
    public async Task<ActionResult<string>> GetUserNameByIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        try
        {
            string? name = await userService.GetUserNameByIdAsync(userId);

            if (name is null)
                return UserNotFound();

            return name;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    #endregion

    #region User Details

    ActionResult UserDetailsIsNull()
        => EntryIsNull("UserDetails");

    [HttpGet("user-details")]
    public async Task<ActionResult<UserDetailsDTO>> GetCurrentUserDetailsAsync()
    {
        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            var currentUserDetails = await userService.GetUserDetailsFromUserAsync(userId);
            return mapper.Map<UserDetailsDTO>(currentUserDetails);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("user-details")]
    public async Task<IActionResult> AddUserDetailsToCurrentUserAsync([FromBody] UserDetailsDTO userDetailsDTO)
    {
        if (userDetailsDTO is null)
            return UserDetailsIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var userDetails = mapper.Map<UserDetails>(userDetailsDTO);
        var serviceResult = await userService.AddUserDetailsToUserAsync(userId, userDetails);

        return HandleServiceResult(serviceResult);
    }

    [HttpPut("user-details")]
    public async Task<IActionResult> UpdateCurrentUserUserDetailsAsync([FromBody] UserDetailsDTO userDetailsDTO)
    {
        if (userDetailsDTO is null)
            return UserDetailsIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var userDetails = mapper.Map<UserDetails>(userDetailsDTO);
        var serviceResult = await userService.UpdateUserDetailsFromUserAsync(userId, userDetails);

        return HandleServiceResult(serviceResult);
    }

    #endregion

    #region UserModels

    [HttpGet($"user-muscle_sizes")]
    public async Task<ActionResult<ApiResult<MuscleSizeDTO>>> GetCurrentUserMuscleSizesAsync(
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
                return EntryNotFound("User muscle sizes");

            var userMuscleSizeDTOs = userMuscleSizes.Select(m => mapper.Map<MuscleSizeDTO>(m));
            return await ApiResult<MuscleSizeDTO>.CreateAsync(
                userMuscleSizeDTOs,
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
    public async Task<ActionResult<ApiResult<BodyWeightDTO>>> GetCurrentUserBodyWeightsAsync(
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
                return EntryNotFound("User body weights");

            var userBodyWeightDTOs = userBodyWeights.Select(m => mapper.Map<BodyWeightDTO>(m));
            return await ApiResult<BodyWeightDTO>.CreateAsync(
                userBodyWeightDTOs,
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
    public async Task<ActionResult<ApiResult<WorkoutDTO>>> GetCurrentUserWorkoutsAsync(
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
                return EntryNotFound("User workouts");

            var userWorkoutDTOs = userWorkouts.Select(m => mapper.Map<WorkoutDTO>(m));
            return await ApiResult<WorkoutDTO>.CreateAsync(
                userWorkoutDTOs,
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
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetCurrentUserCreatedExercisesAsync(
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
                return EntryNotFound("User created exercises");

            var userCreatedExerciseDTOs = userCreatedExercises.Select(m => mapper.Map<ExerciseDTO>(m));
            return await ApiResult<ExerciseDTO>.CreateAsync(
                userCreatedExerciseDTOs,
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
    public async Task<ActionResult<ApiResult<ExerciseRecordDTO>>> GetCurrentUserExerciseRecordsAsync(
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
                return EntryNotFound("User exercise records");

            var userExerciseRecordDTOs = userExerciseRecords.Select(m => mapper.Map<ExerciseRecordDTO>(m));
            return await ApiResult<ExerciseRecordDTO>.CreateAsync(
                userExerciseRecordDTOs,
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

    [HttpGet($"user-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetCurrentUserEquipmentsAsync(
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
            var userEquipments = await userService.GetUserEquipmentsAsync(currentUserId);

            if (userEquipments is null)
                return EntryNotFound("User equipments");

            var userEquipmentDTOs = userEquipments.Select(m => mapper.Map<EquipmentDTO>(m));
            return await ApiResult<EquipmentDTO>.CreateAsync(
                userEquipmentDTOs,
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

    [Authorize(Roles = Roles.AdminRole)]
    [HttpGet("{userId}/has-password")]
    public async Task<ActionResult<bool>> HasUserPasswordAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        try
        {
            return await userService.HasUserPasswordAsync(userId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangeCurrentUserPasswordAsync(PasswordModel passwordModel)
    {
        if (passwordModel is null)
            return EntryIsNull("Password");

        string currentUserId = httpContextAccessor.GetUserId()!;
        var identityResult = await userService.ChangeUserPasswordAsync(currentUserId, passwordModel.OldPassword!, passwordModel.NewPassword);
        return HandleIdentityResult(identityResult);
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpPost("password")]
    public async Task<IActionResult> CreateUserPasswordAsync(string userId, string newPassword)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        if (string.IsNullOrEmpty(newPassword))
            return BadRequest("Password is null or empty.");

        var identityResult = await userService.AddUserPasswordAsync(userId, newPassword);
        return HandleIdentityResult(identityResult);
    }

    #endregion

    #region Roles

    [HttpGet("current-user-roles")]
    public async Task<ActionResult<IEnumerable<string>>> GetCurrentUserRolesAsync()
    {
        try
        {
            string currentUserId = httpContextAccessor.GetUserId()!;
            var userRoles = await userService.GetUserRolesAsync(currentUserId);
            return userRoles.ToList();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("user-roles/{userId}")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserRolesAsync(string userId)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
                return UserIDIsNullOrEmpty();

            var userRoles = await userService.GetUserRolesAsync(userId);
            return userRoles.ToList();
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

        if (roles is null || roles.Length == 0)
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
            if (users is null)
                return EntryNotFound("Users");

            var userDTOs = users.Select(u => mapper.Map<UserDTO>(u));
            return await ApiResult<UserDTO>.CreateAsync(
                userDTOs.AsQueryable(),
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
