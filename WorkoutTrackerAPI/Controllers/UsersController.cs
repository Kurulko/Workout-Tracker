using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.DTOs;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;
using WorkoutTracker.Application.DTOs.Users;
using WorkoutTracker.Application.DTOs.Workouts.Workouts;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Services;

namespace WorkoutTracker.API.Controllers;

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

        var users = await userService.GetUsersAsync();

        if (users is null)
            return EntryNotFound("Users");

        var userDTOs = users.ToList().Select(mapper.Map<UserDTO>);
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

    [HttpGet("{userId}")]
    [Authorize(Roles = Roles.AdminRole)]
    [ActionName(nameof(GetUserByIdAsync))]
    public async Task<ActionResult<UserDTO>> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        User? user = await userService.GetUserByIdAsync(userId);

        if (user is null)
            return UserNotFound();

        return mapper.Map<UserDTO>(user);
    }

    [HttpGet("current-user")]
    public async Task<ActionResult<UserDTO>> GetCurrentUserAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        User currentUser = (await userService.GetUserByIdAsync(userId))!;
        return mapper.Map<UserDTO>(currentUser);
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpGet("userid-by-name/{username}")]
    public async Task<ActionResult<string>> GetUserIdByUserNameAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
            return UserNameIsNullOrEmpty();

        string? userId = await userService.GetUserIdByUsernameAsync(username);

        if (userId is null)
            return UserNotFound();

        return userId;
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpGet("username")]
    public async Task<ActionResult<User>> GetUserByUsernameAsync(string username)
    {
        if (string.IsNullOrEmpty(username))
            return UserNameIsNullOrEmpty();

        User? user = await userService.GetUserByUsernameAsync(username);

        if (user is null)
            return UserNotFound();

        return user;
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpPost]
    public async Task<IActionResult> AddUserAsync(UserCreationDTO userCreationDTO)
    {
        if (userCreationDTO is null)
            return UserIsNull();

        var user = mapper.Map<User>(userCreationDTO);
        await userService.AddUserAsync(user);

        var userDTO = mapper.Map<UserDTO>(user);
        return CreatedAtAction(nameof(GetUserByIdAsync), new { userId = userDTO.UserId }, userDTO);
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

    [Authorize(Roles = Roles.AdminRole)]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        var identityResult = await userService.DeleteUserAsync(userId);
        return HandleIdentityResult(identityResult);
    }

    [HttpGet("user-exists/{userId}")]
    public async Task<ActionResult<bool>> UserExistsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        return await userService.UserExistsAsync(userId);
    }

    [HttpGet("user-exists-by-username/{userName}")]
    public async Task<ActionResult<bool>> UserExistsByUsernameAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return UserNameIsNullOrEmpty();

        return await userService.UserExistsByUsernameAsync(userName);
    }

    [HttpGet("id-by-name/{name}")]
    public async Task<ActionResult<string>> GetUserIdByUsernameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return UserNameIsNullOrEmpty();

        string? userId = await userService.GetUserIdByUsernameAsync(name);

        if (userId is null)
            return UserNotFound();

        return userId;
    }


    [HttpGet("name-by-id/{userId}")]
    public async Task<ActionResult<string>> GetUserNameByIdAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        string? name = await userService.GetUserNameByIdAsync(userId);

        if (name is null)
            return UserNotFound();

        return name;
    }


    #endregion

    #region User Details

    ActionResult UserDetailsIsNull()
        => EntryIsNull("UserDetails");

    [HttpGet("user-details")]
    public async Task<ActionResult<UserDetailsDTO>> GetCurrentUserDetailsAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        var currentUserDetails = await userService.GetUserDetailsFromUserAsync(userId);
        return mapper.Map<UserDetailsDTO>(currentUserDetails);
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


    #endregion

    #region Password

    [Authorize(Roles = Roles.AdminRole)]
    [HttpGet("{userId}/has-password")]
    public async Task<ActionResult<bool>> HasUserPasswordAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        return await userService.HasUserPasswordAsync(userId);
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
        string currentUserId = httpContextAccessor.GetUserId()!;
        var userRoles = await userService.GetUserRolesAsync(currentUserId);
        return userRoles.ToList();
    }

    [HttpGet("user-roles/{userId}")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserRolesAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmpty();

        var userRoles = await userService.GetUserRolesAsync(userId);
        return userRoles.ToList();
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

    #endregion
}
