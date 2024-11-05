using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Data;
using System.Security.Claims;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Repositories.UserRepositories;

public class UserRepository
{
    readonly UserManager<User> userManager;
    readonly WorkoutDbContext db;
    public UserRepository(UserManager<User> userManager, WorkoutDbContext db)
    {
        this.userManager = userManager;
        this.db = db;
    }

    IQueryable<User> Users => userManager.Users;

    static IdentityResult UserNotFoundResult => IdentityResultExtentions.Failed("User not found.");
    static IdentityResult UserIDIsNullOrEmptyResult => IdentityResultExtentions.Failed("User ID cannot not be null or empty.");

    #region CRUD

    public virtual async Task<User> AddUserAsync(User user)
    {
        User? existingUser = await GetUserByIdAsync(user.Id);

        if (existingUser is null)
        {
            if (await UserExistsByUsernameAsync(user.UserName))
                throw new DbUpdateException("User name must be unique.");

            await userManager.CreateAsync(user);
            return user;
        }

        return existingUser;
    }

    public virtual async Task<IdentityResult> CreateUserAsync(User user, string password)
        => await userManager.CreateAsync(user, password);

    public virtual async Task<IdentityResult> UpdateUserAsync(User user)
    {
        if (string.IsNullOrEmpty(user.Id))
            return UserIDIsNullOrEmptyResult;

        User? existingUser = await GetUserByIdAsync(user.Id);

        if (existingUser is not null)
        {
            if (existingUser.UserName != user.UserName)
                await userManager.SetUserNameAsync(existingUser, user.UserName);

            if (existingUser.Email != user.Email)
                await userManager.SetEmailAsync(existingUser, user.Email);

            return await userManager.UpdateAsync(existingUser);
        }

        return UserNotFoundResult;
    }

    public virtual async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return UserIDIsNullOrEmptyResult;

        User? user = await GetUserByIdAsync(userId);

        if (user is not null)
            return await userManager.DeleteAsync(user);

        return UserNotFoundResult;
    }

    public virtual async Task<User?> GetUserByUsernameAsync(string userName)
        => await userManager.FindByNameAsync(userName);
    public virtual async Task<User?> GetUserByEmailAsync(string email)
        => await userManager.FindByEmailAsync(email);

    public virtual async Task<IQueryable<User>> GetUsersAsync()
        => await Task.FromResult(Users);

    public virtual async Task<User?> GetUserByIdAsync(string userId)
        => await userManager.FindByIdAsync(userId);

    public virtual async Task<bool> AnyUsersAsync()
        => await Users.AnyAsync();

    public virtual async Task<bool> UserExistsAsync(string userId)
        => await Users.AnyAsync(u => u.Id == userId);

    public virtual async Task<bool> UserExistsByUsernameAsync(string userName)
        => await Users.AnyAsync(r => r.UserName.ToLower() == userName.ToLower());

    public virtual async Task<bool> UserExistsByEmailAsync(string email)
        => await Users.AnyAsync(r => r.Email.ToLower() == email.ToLower());

    #endregion

    #region User Models

    public virtual async Task<IQueryable<ExerciseRecord>?> GetUserExerciseRecordsAsync(string userId)
    {
        User userWithExerciseRecords = await db.Users.Include(u => u.ExerciseRecords).SingleAsync(u => u.Id == userId);
        return userWithExerciseRecords.ExerciseRecords?.AsQueryable();
    }

    public virtual async Task<IQueryable<MuscleSize>?> GetUserMuscleSizesAsync(string userId)
    {
        User userWithMuscleSizes = await db.Users.Include(u => u.MuscleSizes).SingleAsync(u => u.Id == userId);
        return userWithMuscleSizes.MuscleSizes?.AsQueryable();
    }

    public virtual async Task<IQueryable<BodyWeight>?> GetUserBodyWeightsAsync(string userId)
    {
        User userWithBodyWeights = await db.Users.Include(u => u.BodyWeights).SingleAsync(u => u.Id == userId);
        return userWithBodyWeights.BodyWeights?.AsQueryable();
    }

    public virtual async Task<IQueryable<Workout>?> GetUserWorkoutsAsync(string userId)
    {
        User userWithWorkouts = await db.Users.Include(u => u.Workouts).SingleAsync(u => u.Id == userId);
        return userWithWorkouts.Workouts?.AsQueryable();
    }

    public virtual async Task<IQueryable<Exercise>?> GetUserCreatedExercisesAsync(string userId)
    {
        User userWithCreatedExercises = await db.Users.Include(u => u.CreatedExercises).SingleAsync(u => u.Id == userId);
        return userWithCreatedExercises.CreatedExercises?.AsQueryable();
    }

    public virtual async Task<IQueryable<Equipment>?> GetUserEquipmentsAsync(string userId)
    {
        User userWithCreatedExercises = await db.Users.Include(u => u.UserEquipments).SingleAsync(u => u.Id == userId);
        return userWithCreatedExercises.UserEquipments?.AsQueryable();
    }

    #endregion

    #region Password

    public virtual async Task<IdentityResult> ChangeUserPasswordAsync(string userId, string oldPassword, string newPassword)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return UserNotFoundResult;

        return await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
    }

    public virtual async Task<IdentityResult> AddUserPasswordAsync(string userId, string newPassword)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return UserNotFoundResult;

        return await userManager.AddPasswordAsync(user, newPassword);
    }

    public virtual async Task<bool> HasUserPasswordAsync(string userId)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
           throw new NotFoundException(nameof(User));

        return await userManager.HasPasswordAsync(user);
    }

    #endregion

    #region Roles

    public virtual async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            throw new NotFoundException(nameof(User));

        return await userManager.GetRolesAsync(user);
    }

    public virtual async Task<IdentityResult> AddRolesToUserAsync(string userId, string[] roles)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return UserNotFoundResult;

        return await userManager.AddToRolesAsync(user, roles);
    }

    public virtual async Task<IdentityResult> DeleteRoleFromUserAsync(string userId, string roleName)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return UserNotFoundResult;

        return await userManager.RemoveFromRoleAsync(user, roleName);
    }

    #endregion
}
