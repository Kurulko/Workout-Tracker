using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;

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

    IQueryable<User> users => userManager.Users;

    IdentityResult userNotFoundResult => IdentityResult.Failed(new IdentityError()
    {
        Description = "User not found!"
    });

    #region CRUD

    public async Task<User> AddUserAsync(User user)
    {
        User? existingUser = await GetUserByIdAsync(user.Id);

        if (existingUser is null)
        {
            await userManager.CreateAsync(user);
            return user;
        }

        return existingUser;
    }

    public async Task<IdentityResult> CreateUserAsync(User user, string password)
        => await userManager.CreateAsync(user, password);

    public async Task<IdentityResult> UpdateUserAsync(User user)
    {
        User? existingUser = await GetUserByIdAsync(user.Id);

        if (existingUser is not null)
        {
            if (existingUser.UserName != user.UserName)
                await userManager.SetUserNameAsync(existingUser, user.UserName);

            if (existingUser.Email != user.Email)
                await userManager.SetEmailAsync(existingUser, user.Email);

            return await userManager.UpdateAsync(existingUser);
        }

        return userNotFoundResult;
    }

    public async Task<IdentityResult> DeleteUserAsync(string userId)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is not null)
            return await userManager.DeleteAsync(user);

        return userNotFoundResult;
    }

    public async Task<User?> GetUserByClaimsAsync(ClaimsPrincipal claims)
        => await GetUserByUsernameAsync(claims.Identity?.Name!);

    public async Task<string?> GetUserIdByUsernameAsync(string userName)
        => (await GetUserByUsernameAsync(userName))?.Id;

    public async Task<User?> GetUserByUsernameAsync(string userName)
        => await users.SingleOrDefaultAsync(u => u.UserName.ToLower() == userName.ToLower());

    public async Task<IQueryable<User>> GetUsersAsync()
        => await Task.FromResult(users);

    public async Task<User?> GetUserByIdAsync(string userId)
        => await users.SingleOrDefaultAsync(u => u.Id == userId);

    public async Task<bool> UserExistsAsync(string userId)
        => await users.AnyAsync(u => u.Id == userId);

    public async Task<bool> UserExistsByUsernameAsync(string userName)
        => await users.AnyAsync(r => r.UserName == userName);

    #endregion

    #region User Models

    public async Task<IQueryable<ExerciseRecord>?> GetUserExerciseRecordsAsync(string userId)
    {
        User userWithExerciseRecords = await db.Users.Include(u => u.ExerciseRecords).SingleAsync(u => u.Id == userId);
        return userWithExerciseRecords.ExerciseRecords?.AsQueryable();
    }

    public async Task<IQueryable<MuscleSize>?> GetUserMuscleSizesAsync(string userId)
    {
        User userWithMuscleSizes = await db.Users.Include(u => u.MuscleSizes).SingleAsync(u => u.Id == userId);
        return userWithMuscleSizes.MuscleSizes?.AsQueryable();
    }

    public async Task<IQueryable<BodyWeight>?> GetUserBodyWeightsAsync(string userId)
    {
        User userWithBodyWeights = await db.Users.Include(u => u.BodyWeights).SingleAsync(u => u.Id == userId);
        return userWithBodyWeights.BodyWeights?.AsQueryable();
    }

    public async Task<IQueryable<Workout>?> GetUserWorkoutsAsync(string userId)
    {
        User userWithWorkouts = await db.Users.Include(u => u.Workouts).SingleAsync(u => u.Id == userId);
        return userWithWorkouts.Workouts?.AsQueryable();
    }

    public async Task<IQueryable<Exercise>?> GetUserCreatedExercisesAsync(string userId)
    {
        User userWithCreatedExercises = await db.Users.Include(u => u.CreatedExercises).SingleAsync(u => u.Id == userId);
        return userWithCreatedExercises.CreatedExercises?.AsQueryable();
    }

    #endregion

    #region Password

    public async Task<IdentityResult> ChangeUserPasswordAsync(string userId, PasswordModel passwordModel)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return userNotFoundResult;

        return await userManager.ChangePasswordAsync(user, passwordModel.OldPassword!, passwordModel.NewPassword);
    }

    public async Task<IdentityResult> AddUserPasswordAsync(string userId, string newPassword)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return userNotFoundResult;

        return await userManager.AddPasswordAsync(user!, newPassword);
    }

    public async Task<bool> HasUserPasswordAsync(string userId)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return false;

        return await userManager.HasPasswordAsync(user);
    }

    #endregion

    #region Roles

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            throw new NotFoundException(nameof(User));

        return await userManager.GetRolesAsync(user);
    }

    public async Task<IEnumerable<string>?> GetRolesAsync(string userId)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return null;

        return await userManager.GetRolesAsync(user);
    }

    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
    {
        var allUser = await GetUsersAsync();

        List<User> usersByRole = new();
        foreach (User user in allUser)
        {
            var userRoles = (await GetRolesAsync(user.Id))!;
            if (userRoles.Contains(roleName))
                usersByRole.Add(user);
        }

        return usersByRole;
    }

    public async Task<IdentityResult> AddRoleToUserAsync(string userId, string roleName)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return userNotFoundResult;

        return await userManager.AddToRoleAsync(user, roleName);
    }

    public async Task<IdentityResult> DeleteRoleFromUserAsync(string userId, string roleName)
    {
        User? user = await GetUserByIdAsync(userId);

        if (user is null)
            return userNotFoundResult;

        return await userManager.RemoveFromRoleAsync(user, roleName);
    }

    #endregion
}
