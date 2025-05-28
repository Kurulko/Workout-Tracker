using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Infrastructure.Identity.Entities;

namespace WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;

public interface IUserRepository
{
    #region CRUD

    Task<User> AddUserAsync(User user);
    Task CreateUserAsync(User user, string password);

    Task UpdateUserAsync(User user);
    Task DeleteUserAsync(string userId);

    Task<User?> GetUserByUsernameAsync(string userName);
    Task<User?> GetUserByEmailAsync(string email);
    IQueryable<User> GetUsers();
    Task<User?> GetUserByIdAsync(string userId);

    Task<string?> GetUserIdByUsernameAsync(string userName);
    Task<string?> GetUserNameByIdAsync(string userId);

    Task<bool> AnyUsersAsync();
    Task<bool> UserExistsAsync(string userId);
    Task<bool> UserExistsByUsernameAsync(string userName);
    Task<bool> UserExistsByEmailAsync(string email);

    #endregion

    #region User Models

    Task<UserDetails?> GetUserDetailsFromUserAsync(string userId);
    Task<IQueryable<MuscleSize>?> GetUserMuscleSizesAsync(string userId);
    Task<IQueryable<BodyWeight>?> GetUserBodyWeightsAsync(string userId);
    Task<IQueryable<Workout>?> GetUserWorkoutsAsync(string userId);
    Task<IQueryable<WorkoutRecord>?> GetUserWorkoutRecordsAsync(string userId);
    Task<IQueryable<Exercise>?> GetUserCreatedExercisesAsync(string userId);
    Task<IQueryable<Equipment>?> GetUserEquipmentsAsync(string userId);

    #endregion

    #region Password

    Task ChangeUserPasswordAsync(string userId, string oldPassword, string newPassword);
    Task AddUserPasswordAsync(string userId, string newPassword);
    Task<bool> HasUserPasswordAsync(string userId);

    #endregion

    #region Roles

    Task<IEnumerable<string>> GetUserRolesAsync(string userId);
    Task AddRolesToUserAsync(string userId, string[] roles);
    Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
    Task DeleteRoleFromUserAsync(string userId, string roleName);

    #endregion
}
