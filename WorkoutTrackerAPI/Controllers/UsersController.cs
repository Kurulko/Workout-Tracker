using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.DTOs.BodyWeights;
using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;
using WorkoutTracker.Application.DTOs.Users;
using WorkoutTracker.Application.DTOs.Workouts.Workouts;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Services;

namespace WorkoutTracker.API.Controllers;

[Authorize]
public class UsersController : APIController
{
    readonly IUserService userService;
    readonly IMapper mapper;
    readonly IHttpContextAccessor httpContextAccessor;
    public UsersController (
        IUserService userService, 
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    )
    {
        this.userService = userService;
        this.httpContextAccessor = httpContextAccessor;
        this.mapper = mapper;
    }

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
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var users = await userService.GetUsersAsync();

        var userDTOs = users.Select(mapper.Map<UserDTO>);
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
        if (!IsValidID(userId))
            return UserIDIsNullOrEmpty();

        var user = await userService.GetUserByIdAsync(userId);
        return ToUserDTO(user);
    }

    [HttpGet("current-user")]
    public async Task<ActionResult<UserDTO>> GetCurrentUserAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        User currentUser = (await userService.GetUserByIdAsync(userId))!;
        return ToUserDTO(currentUser);
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpGet("userid-by-name/{username}")]
    public async Task<ActionResult<string>> GetUserIdByUserNameAsync(string username)
    {
        if (!IsNameValid(username))
            return UserNameIsNullOrEmpty();

        string? userId = await userService.GetUserIdByUsernameAsync(username);

        if (userId is null)
            return UserNotFound();

        return userId;
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpGet("username")]
    public async Task<ActionResult<UserDTO>> GetUserByUsernameAsync(string username)
    {
        if (!IsNameValid(username))
            return UserNameIsNullOrEmpty();

        var user = await userService.GetUserByUsernameAsync(username);
        return ToUserDTO(user);
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

        if (IsPasswordValid(password))
            return PasswordIsNullOrEmpty();

        var user = mapper.Map<User>(userCreationDTO);
        await userService.CreateUserAsync(user, password);

        var userDTO = mapper.Map<UserDTO>(user);
        return CreatedAtAction(nameof(GetUserByIdAsync), new { userId = userDTO.UserId }, userDTO);
    }

    [HttpPut("{userId}")]
    public async Task<IActionResult> UpdateUserAsync(string userId, UserUpdateDTO userDTO)
    {
        if (!IsValidID(userId))
            return UserIDIsNullOrEmpty();

        if (userDTO is null)
            return UserIsNull();

        if (!AreIdsEqual(userId, userDTO.UserId))
            return EntryIDsNotMatch(nameof(User));

        string currentUserId = httpContextAccessor.GetUserId()!;
        var roles = await userService.GetUserRolesAsync(currentUserId);
        bool isAdmin = roles.Contains(Roles.AdminRole);

        bool isCurrentUserUpdating = userDTO.UserId == currentUserId;

        if (!isAdmin && !isCurrentUserUpdating)
            return Forbid();

        var user = mapper.Map<User>(userDTO);
        await userService.UpdateUserAsync(user);
        return Ok();
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpDelete("{userId}")]
    public async Task<IActionResult> DeleteUserAsync(string userId)
    {
        if (!IsValidID(userId))
            return UserIDIsNullOrEmpty();

        await userService.DeleteUserAsync(userId);
        return Ok();
    }

    [HttpGet("id-by-name/{name}")]
    public async Task<ActionResult<string>> GetUserIdByUsernameAsync(string name)
    {
        if (!IsNameValid(name))
            return UserNameIsNullOrEmpty();

        string? userId = await userService.GetUserIdByUsernameAsync(name);

        if (userId is null)
            return UserNotFound();

        return userId;
    }

    [HttpGet("name-by-id/{userId}")]
    public async Task<ActionResult<string>> GetUserNameByIdAsync(string userId)
    {
        if (!IsValidID(userId))
            return UserIDIsNullOrEmpty();

        string? name = await userService.GetUserNameByIdAsync(userId);

        if (name is null)
            return UserNotFound();

        return name;
    }

    #endregion

    #region User Details

    [HttpGet("user-details")]
    public async Task<ActionResult<UserDetailsDTO>> GetCurrentUserDetailsAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        var currentUserDetails = await userService.GetUserDetailsFromUserAsync(userId);
        return mapper.Map<UserDetailsDTO>(currentUserDetails);
    }

    [HttpPost("user-details")]
    public async Task<IActionResult> AddUserDetailsToCurrentUserAsync([FromBody] UserDetailsDTO userDetailsDTO, CancellationToken cancellationToken)
    {
        if (userDetailsDTO is null)
            return UserDetailsIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var userDetails = mapper.Map<UserDetails>(userDetailsDTO);
        await userService.AddUserDetailsToUserAsync(userId, userDetails, cancellationToken);

        return Ok();
    }

    [HttpPut("user-details")]
    public async Task<IActionResult> UpdateCurrentUserUserDetailsAsync([FromBody] UserDetailsDTO userDetailsDTO, CancellationToken cancellationToken)
    {
        if (userDetailsDTO is null)
            return UserDetailsIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var userDetails = mapper.Map<UserDetails>(userDetailsDTO);
        await userService.UpdateUserDetailsFromUserAsync(userId, userDetails, cancellationToken);

        return Ok();
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
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string currentUserId = httpContextAccessor.GetUserId()!;
        var userMuscleSizes = await userService.GetUserMuscleSizesAsync(currentUserId);

        if (userMuscleSizes is null)
            return EntryNotFound("User muscle sizes");

        var userMuscleSizeDTOs = userMuscleSizes.Select(mapper.Map<MuscleSizeDTO>);
        return await ApiResult<MuscleSizeDTO>.CreateAsync(
            userMuscleSizeDTOs.AsQueryable(),
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
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string currentUserId = httpContextAccessor.GetUserId()!;
        var userBodyWeights = await userService.GetUserBodyWeightsAsync(currentUserId);

        if (userBodyWeights is null)
            return EntryNotFound("User body weights");

        var userBodyWeightDTOs = userBodyWeights.Select(mapper.Map<BodyWeightDTO>);
        return await ApiResult<BodyWeightDTO>.CreateAsync(
            userBodyWeightDTOs.AsQueryable(),
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
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string currentUserId = httpContextAccessor.GetUserId()!;
        var userWorkouts = await userService.GetUserWorkoutsAsync(currentUserId);

        if (userWorkouts is null)
            return EntryNotFound("User workouts");

        var userWorkoutDTOs = userWorkouts.Select(mapper.Map<WorkoutDTO>);
        return await ApiResult<WorkoutDTO>.CreateAsync(
            userWorkoutDTOs.AsQueryable(),
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
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string currentUserId = httpContextAccessor.GetUserId()!;
        var userCreatedExercises = await userService.GetUserCreatedExercisesAsync(currentUserId);

        if (userCreatedExercises is null)
            return EntryNotFound("User created exercises");

        var userCreatedExerciseDTOs = userCreatedExercises.Select(mapper.Map<ExerciseDTO>);
        return await ApiResult<ExerciseDTO>.CreateAsync(
            userCreatedExerciseDTOs.AsQueryable(),
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
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string currentUserId = httpContextAccessor.GetUserId()!;
        var userEquipments = await userService.GetUserEquipmentsAsync(currentUserId);

        if (userEquipments is null)
            return EntryNotFound("User equipments");

        var userEquipmentDTOs = userEquipments.Select(mapper.Map<EquipmentDTO>);
        return await ApiResult<EquipmentDTO>.CreateAsync(
            userEquipmentDTOs.AsQueryable(),
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
        if (!IsValidID(userId))
            return UserIDIsNullOrEmpty();

        return await userService.HasUserPasswordAsync(userId);
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangeCurrentUserPasswordAsync(PasswordModel passwordModel)
    {
        if (passwordModel is null)
            return EntryIsNull("Password");

        string currentUserId = httpContextAccessor.GetUserId()!;
        await userService.ChangeUserPasswordAsync(currentUserId, passwordModel.OldPassword!, passwordModel.NewPassword);
        return Ok();
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpPost("password")]
    public async Task<IActionResult> CreateUserPasswordAsync(string userId, string newPassword)
    {
        if (!IsValidID(userId))
            return UserIDIsNullOrEmpty();

        if (IsPasswordValid(newPassword))
            return PasswordIsNullOrEmpty();

        await userService.AddUserPasswordAsync(userId, newPassword);
        return Ok();
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
        if (!IsValidID(userId))
            return UserIDIsNullOrEmpty();

        var userRoles = await userService.GetUserRolesAsync(userId);
        return userRoles.ToList();
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpPost("{userId}/role")]
    public async Task<IActionResult> AddRolesToUserAsync(string userId, string[] roles)
    {
        if (!IsValidID(userId))
            return UserIDIsNullOrEmpty();

        if (roles is null || roles.Length == 0)
            return BadRequest("User cannot have no roles.");

        await userService.AddRolesToUserAsync(userId, roles);
        return Ok();
    }

    [Authorize(Roles = Roles.AdminRole)]
    [HttpDelete("{userId}/{roleName}")]
    public async Task<IActionResult> DeleteRoleFromUserAsync(string userId, string roleName)
    {
        if (!IsValidID(userId))
            return UserIDIsNullOrEmpty();

        if (!IsNameValid(roleName))
            return RoleNameIsNullOrEmpty();

        await userService.DeleteRoleFromUserAsync(userId, roleName);
        return Ok();
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
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        if (!IsNameValid(roleName))
            return RoleNameIsNullOrEmpty();

        var users = await userService.GetUsersByRoleAsync(roleName);

        var userDTOs = users.Select(mapper.Map<UserDTO>);
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

    static bool IsValidID(string id) => !string.IsNullOrEmpty(id);
    static bool IsNameValid(string name) => !string.IsNullOrEmpty(name);
    static bool IsPasswordValid(string password) => !string.IsNullOrEmpty(password);
    static bool AreIdsEqual(string id1, string id2) => id1 == id2;

    ActionResult UserIDIsNullOrEmpty()
        => BadRequest("User ID is null or empty.");
    ActionResult UserNameIsNullOrEmpty()
        => BadRequest("User name is null or empty.");
    ActionResult RoleNameIsNullOrEmpty()
        => BadRequest("Role name is null or empty.");
    ActionResult PasswordIsNullOrEmpty()
        => BadRequest("Password is null or empty.");
    ActionResult UserIsNull()
        => EntryIsNull(nameof(User));
    ActionResult UserNotFound()
        => EntryNotFound(nameof(User));
    ActionResult UserDetailsIsNull()
        => EntryIsNull(nameof(UserDetails));

    ActionResult<UserDTO> ToUserDTO(User? user)
    {
        if (user is null)
            return NotFound("User not found.");

        var userDTO = mapper.Map<UserDTO>(user);
        return userDTO;
    }
}
