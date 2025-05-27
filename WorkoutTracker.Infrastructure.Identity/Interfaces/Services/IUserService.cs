using System.Security.Claims;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Infrastructure.Identity.Interfaces.Services;

public interface IUserService : IBaseService
{
    #region CRUD

    Task<User> AddUserAsync(User user);
    Task CreateUserAsync(User user, string password);

    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(string userId);

    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByUsernameAsync(string userName);
    Task<User?> GetUserByClaimsAsync(ClaimsPrincipal claims);
    Task<IEnumerable<User>> GetUsersAsync();

    Task<string?> GetUserIdByUsernameAsync(string userName);
    Task<string?> GetUserNameByIdAsync(string userId);

    #endregion

    #region User Details

    Task<UserDetails?> GetUserDetailsFromUserAsync(string userId);
    Task AddUserDetailsToUserAsync(string userId, UserDetails userDetails, CancellationToken cancellationToken = default);
    Task UpdateUserDetailsFromUserAsync(string userId, UserDetails userDetails, CancellationToken cancellationToken = default);

    #endregion

    #region User Models

    Task<IEnumerable<MuscleSize>?> GetUserMuscleSizesAsync(string userId);
    Task<IEnumerable<BodyWeight>?> GetUserBodyWeightsAsync(string userId);
    Task<IEnumerable<Workout>?> GetUserWorkoutsAsync(string userId);
    Task<IEnumerable<Exercise>?> GetUserCreatedExercisesAsync(string userId);
    Task<IEnumerable<Equipment>?> GetUserEquipmentsAsync(string userId);

    #endregion

    #region Password

    Task ChangeUserPasswordAsync(string userId, string oldPassword, string newPassword);
    Task AddUserPasswordAsync(string userId, string newPassword);
    Task<bool> HasUserPasswordAsync(string userId);

    #endregion

    #region Roles

    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    Task AddRolesToUserAsync(string userId, string[] roles);
    Task DeleteRoleFromUserAsync(string userId, string roleName);

    #endregion
}
