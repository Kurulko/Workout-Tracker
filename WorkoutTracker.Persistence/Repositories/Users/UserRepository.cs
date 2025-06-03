using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Application.Common.Validators;

namespace WorkoutTracker.Persistence.Repositories.Users;

public class UserRepository : IUserRepository
{
    readonly UserManager<User> userManager;
    public UserRepository(UserManager<User> userManager)
        => this.userManager = userManager;

    IQueryable<User> Users => userManager.Users;

    #region CRUD

    readonly string userEntityName = nameof(User);

    public async Task<User> AddUserAsync(User user)
    {
        var existingUser = await GetByIdAsync(user.Id);

        if (existingUser is null)
        {
            TrimUserName(user);
            await ArgumentValidator.EnsureNonExistsByNameAsync(GetByUsernameAsync, user.UserName!);

            var result = await userManager.CreateAsync(user);
            ArgumentValidator.ThrowIfNotSucceeded("add", "user", result);

            return user;
        }

        return existingUser;
    }

    public async Task CreateUserAsync(User user, string password)
    {
        var existingUser = await GetByIdAsync(user.Id);

        if (existingUser is null)
        {
            TrimUserName(user);

            ArgumentValidator.ThrowIfArgumentNullOrEmpty(password, nameof(password));
            await ArgumentValidator.EnsureNonExistsByNameAsync(GetByUsernameAsync, user.UserName!);

            var result = await userManager.CreateAsync(user, password);
            ArgumentValidator.ThrowIfNotSucceeded("create", "user", result);
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        var existingUser = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, user.Id, userEntityName);
        TrimUserName(user);

        if (existingUser.UserName != user.UserName)
            await userManager.SetUserNameAsync(existingUser, user.UserName);

        if (existingUser.Email != user.Email)
            await userManager.SetEmailAsync(existingUser, user.Email);

        existingUser.CountOfTrainings = user.CountOfTrainings;
        existingUser.Registered = user.Registered;
        existingUser.StartedWorkingOut = user.StartedWorkingOut;

        var result = await userManager.UpdateAsync(existingUser);
        ArgumentValidator.ThrowIfNotSucceeded("update", userEntityName, result);
    }

    public async Task DeleteUserAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        var user = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, userId, userEntityName);

        var result = await userManager.DeleteAsync(user);
        ArgumentValidator.ThrowIfNotSucceeded("delete", "user", result);
    }

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        return await GetByIdAsync(userId);
    }

    public async Task<User?> GetUserByUsernameAsync(string userName)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(userName, nameof(User.UserName));

        return await GetByUsernameAsync(userName);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(email, nameof(User.Email));

        return await GetByEmailAsync(email);

    }

    public IQueryable<User> GetUsers()
        => Users;

    public async Task<bool> AnyUsersAsync()
        => await Users.AnyAsync();

    public async Task<bool> UserExistsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        return await GetByIdAsync(userId) != null;
    }

    public async Task<bool> UserExistsByUsernameAsync(string userName)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(userName, nameof(User.UserName));

        return await GetByUsernameAsync(userName) != null;
    }

    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(email, nameof(User.Email));

        return await GetByEmailAsync(email) != null;
    }

    public async Task<string?> GetUserIdByUsernameAsync(string userName)
    {
        var userByUsername = await GetUserByUsernameAsync(userName);
        return userByUsername?.Id;
    }

    public async Task<string?> GetUserNameByIdAsync(string userId)
    {
        var userById = await GetUserByIdAsync(userId);
        return userById?.UserName;
    }

    #endregion

    #region User Models

    public async Task<UserDetails?> GetUserDetailsFromUserAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        User userWithUserDetails = await Users.Include(u => u.UserDetails).SingleAsync(u => u.Id == userId);
        return userWithUserDetails.UserDetails;
    }

    public async Task<IQueryable<MuscleSize>?> GetUserMuscleSizesAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        User userWithMuscleSizes = await Users.Include(u => u.MuscleSizes).SingleAsync(u => u.Id == userId);
        return userWithMuscleSizes.MuscleSizes?.AsQueryable();
    }

    public async Task<IQueryable<BodyWeight>?> GetUserBodyWeightsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        User userWithBodyWeights = await Users.Include(u => u.BodyWeights).SingleAsync(u => u.Id == userId);
        return userWithBodyWeights.BodyWeights?.AsQueryable();
    }

    public async Task<IQueryable<Workout>?> GetUserWorkoutsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        User userWithWorkouts = await Users
            .Include(u => u.Workouts)!
            .ThenInclude(u => u.WorkoutRecords)!
            .ThenInclude(u => u.ExerciseRecordGroups)
            .ThenInclude(u => u.ExerciseRecords)
            .SingleAsync(u => u.Id == userId);
        return userWithWorkouts.Workouts?.AsQueryable();
    }

    public async Task<IQueryable<WorkoutRecord>?> GetUserWorkoutRecordsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        User userWithWorkoutRecords = await Users
            .Include(u => u.WorkoutRecords)!
            .ThenInclude(u => u.ExerciseRecordGroups)
            .ThenInclude(u => u.ExerciseRecords)
            .SingleAsync(u => u.Id == userId);
        return userWithWorkoutRecords.WorkoutRecords?.AsQueryable();
    }

    public async Task<IQueryable<Exercise>?> GetUserCreatedExercisesAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        User userWithCreatedExercises = await Users.Include(u => u.CreatedExercises).SingleAsync(u => u.Id == userId);
        return userWithCreatedExercises.CreatedExercises?.AsQueryable();
    }

    public virtual async Task<IQueryable<Equipment>?> GetUserEquipmentsAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        User userWithCreatedExercises = await Users.Include(u => u.UserEquipments).SingleAsync(u => u.Id == userId);
        return userWithCreatedExercises.UserEquipments?.AsQueryable();
    }

    #endregion

    #region Password

    public async Task ChangeUserPasswordAsync(string userId, string oldPassword, string newPassword)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(oldPassword, "Old Password");
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(newPassword, "New Password");

        var user = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, userId, userEntityName);

        var result = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        ArgumentValidator.ThrowIfNotSucceeded("change", "password", result);
    }

    public async Task AddUserPasswordAsync(string userId, string newPassword)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(newPassword, "New Password");

        var user = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, userId, userEntityName);

        var result = await userManager.AddPasswordAsync(user, newPassword);
        ArgumentValidator.ThrowIfNotSucceeded("add", "password", result);
    }

    public async Task<bool> HasUserPasswordAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);
        var user = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, userId, userEntityName);

        return await userManager.HasPasswordAsync(user);
    }

    #endregion

    #region Roles

    public async Task<IEnumerable<string>> GetUserRolesAsync(string userId)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);
        var user = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, userId, userEntityName);

        return await userManager.GetRolesAsync(user);
    }

    public async Task AddRolesToUserAsync(string userId, string[] roles)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        var user = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, userId, userEntityName);

        var result = await userManager.AddToRolesAsync(user, roles);
        ArgumentValidator.ThrowIfNotSucceeded("add", "roles", result);
    }


    public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
    {
        var users = GetUsers();
        var usersByRole = new List<User>();

        foreach (var user in users)
        {
            var userRoles = (await GetUserRolesAsync(user.Id))!;

            if (userRoles.Contains(roleName))
                usersByRole.Add(user);
        }

        return usersByRole;
    }

    public async Task DeleteRoleFromUserAsync(string userId, string roleName)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, userEntityName);

        var user = await ArgumentValidator.EnsureExistsByIdAsync(GetByIdAsync, userId, userEntityName);

        var result = await userManager.RemoveFromRoleAsync(user, roleName);
        ArgumentValidator.ThrowIfNotSucceeded("remove", "role", result);
    }

    #endregion

    async Task<User?> GetByIdAsync(string userId)
        => await userManager.FindByIdAsync(userId);
    async Task<User?> GetByUsernameAsync(string userName)
        => await userManager.FindByNameAsync(userName);
    async Task<User?> GetByEmailAsync(string email)
        => await userManager.FindByEmailAsync(email);

    void TrimUserName(User user)
        => user.UserName = user.UserName!.Trim();
}