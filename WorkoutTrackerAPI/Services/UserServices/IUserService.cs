using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Services.UserServices;

public interface IUserService
{
    #region CRUD

    Task<User> AddUserAsync(User user);
    Task<IdentityResult> CreateUserAsync(User user, string password);

    Task<IdentityResult> UpdateUserAsync(User user);
    Task<IdentityResult> DeleteUserAsync(string userId);

    Task<bool> UserExistsAsync(string userId);
    Task<bool> UserExistsByUsernameAsync(string userName);

    Task<IQueryable<User>> GetUsersAsync();
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByClaimsAsync(ClaimsPrincipal claims);
    Task<User?> GetUserByUsernameAsync(string userName);
    Task<string?> GetUserIdByUsernameAsync(string userName);

    #endregion

    #region User Models

    Task<IQueryable<ExerciseRecord>?> GetUserExerciseRecordsAsync(string userId);
    Task<IQueryable<MuscleSize>?> GetUserMuscleSizesAsync(string userId);
    Task<IQueryable<BodyWeight>?> GetUserBodyWeightsAsync(string userId);
    Task<IQueryable<Workout>?> GetUserWorkoutsAsync(string userId);
    Task<IQueryable<Exercise>?> GetUserCreatedExercisesAsync(string userId);

    #endregion

    #region Password

    Task<IdentityResult> ChangeUserPasswordAsync(string userId, PasswordModel passwordModel);
    Task<IdentityResult> AddUserPasswordAsync(string userId, string newPassword);
    //Task<bool> HasUserPasswordAsync(string userId);

    #endregion

    #region Roles

    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    Task<IEnumerable<string>?> GetRolesAsync(string userId);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    Task<IdentityResult> AddRoleToUserAsync(string userId, string roleName);
    Task<IdentityResult> DeleteRoleFromUserAsync(string userId, string roleName);

    #endregion
}
