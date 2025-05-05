using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Repositories.UserRepositories;
using Xunit;
using Microsoft.AspNetCore.Identity;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Data.Account;
using WorkoutTracker.API.Extentions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using WorkoutTracker.API.Exceptions;
using WorkoutTracker.API.Data.Models.UserModels;

namespace WorkoutTracker.API.Tests.Repositories.UserRepositories;

public class UserRepository_Tests
{
    static User CreateUser(string name, string email)
    {
        User user = new()
        {
            UserName = name,
            Email = email,
            Registered = DateTime.Now
        };

        return user;
    }

    static User GetValidUser()
    {
        return CreateUser("User", "user@gmail.com");
    }

    static IEnumerable<User> GetValidUsers()
    {
        var user_User = CreateUser("User", "user@gmail.com");
        var admin_User = CreateUser("Admin", "admin@gmail.com");

        return new[] { user_User, admin_User }; ;
    }

    #region CRUD

    [Fact]
    public async Task AddUser_ShouldReturnNewUser()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();

        //Act
        var newUser = await userRepository.AddUserAsync(validUser);

        //Assert
        Assert.NotNull(newUser);

        var userById = await userRepository.GetUserByIdAsync(newUser.Id);
        Assert.NotNull(userById);

        Assert.Equal(newUser, userById);
    }

    [Fact]
    public async Task CreateUser_ShouldCreateNewUserSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        string password = "P@$$w0rd";

        //Act
        var result = await userRepository.CreateUserAsync(validUser, password);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AddUser_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        var sameValidUser = GetValidUser();

        //Act & Assert
        var ex = await Assert.ThrowsAsync<DbUpdateException>(async () => await userRepository.AddUserAsync(sameValidUser));
        Assert.Equal("User name must be unique.", ex.Message);
    }


    [Fact]
    public async Task DeleteUser_ShouldDeleteUserSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        //Act
        var result = await userRepository.DeleteUserAsync(validUser.Id);

        //Assert
        Assert.True(result.Succeeded);

        var userById = await userRepository.GetUserByIdAsync(validUser.Id);
        Assert.Null(userById);
    }

    [Fact]
    public async Task DeleteUser_UserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        //Act
        var result = await userRepository.DeleteUserAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.False(result.Succeeded);

        var isUserNotFound = result.ErrorExists("User not found.");
        Assert.True(isUserNotFound);
    }

    [Fact]
    public async Task DeleteUser_IncorrectUserID()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        //Act
        var result = await userRepository.DeleteUserAsync(string.Empty);

        //Assert
        Assert.False(result.Succeeded);

        var isUserIDIsNullOrEmpty = result.ErrorExists("User ID cannot not be null or empty.");
        Assert.True(isUserIDIsNullOrEmpty);
    }

    [Fact]
    public async Task GetUsers_ShouldReturnUsers()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUsers = GetValidUsers();
        foreach (var user in validUsers)
            await userRepository.AddUserAsync(user);

        //Act
        var users = await userRepository.GetUsersAsync();

        //Assert
        Assert.NotNull(users);
        Assert.NotEmpty(users);
        Assert.Equal(validUsers.Count(), users.Count());
    }

    [Fact]
    public async Task GetUsers_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        //Act
        var users = await userRepository.GetUsersAsync();

        //Assert
        Assert.NotNull(users);
        Assert.Empty(users);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnUserById()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        //Act
        var userById = await userRepository.GetUserByIdAsync(validUser.Id);

        //Assert
        Assert.NotNull(userById);
        Assert.Equal(validUser, userById);
    }

    [Fact]
    public async Task GetUserById_ShouldReturnNull()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        //Act
        var userById = await userRepository.GetUserByIdAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.Null(userById);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnUserByName()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        //Act
        var userByName = await userRepository.GetUserByUsernameAsync(validUser.UserName);

        //Assert
        Assert.NotNull(userByName);
        Assert.Equal(validUser, userByName);
    }

    [Fact]
    public async Task GetUserByUsername_ShouldReturnNull()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        //Act
        var userByName = await userRepository.GetUserByUsernameAsync("Non-existence name");

        //Assert
        Assert.Null(userByName);
    }

    [Fact]
    public async Task UserExists_ShouldReturnTrue()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        //Act
        var userExists = await userRepository.UserExistsAsync(validUser.Id);

        //Assert
        Assert.True(userExists);
    }

    [Fact]
    public async Task UserExists_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        //Act
        var userExists = await userRepository.UserExistsAsync(Guid.NewGuid().ToString());

        //Assert
        Assert.False(userExists);
    }

    [Fact]
    public async Task UserExistsByUsername_ShouldReturnTrue()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        //Act
        var userExistsByName = await userRepository.UserExistsByUsernameAsync(validUser.UserName);

        //Assert
        Assert.True(userExistsByName);
    }

    [Fact]
    public async Task UserExistsByName_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        //Act
        var userExistsByName = await userRepository.UserExistsByUsernameAsync("Non-existence name");

        //Assert
        Assert.False(userExistsByName);
    }


    [Fact]
    public async Task UpdateUser_ShouldUpdateUserSuccessfully()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        validUser.UserName = "New User Name";

        //Act
        await userRepository.UpdateUserAsync(validUser);

        //Assert
        var updatedUser = await userRepository.GetUserByIdAsync(validUser.Id);

        Assert.NotNull(updatedUser);
        Assert.Equal(updatedUser, validUser);
    }

    [Fact]
    public async Task UpdateUser_UserNotFound()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        validUser.Id = Guid.NewGuid().ToString();

        //Act
        var result = await userRepository.UpdateUserAsync(validUser);

        //Assert
        Assert.False(result.Succeeded);

        var isUserNotFound = result.ErrorExists("User not found.");
        Assert.True(isUserNotFound);
    }

    [Fact]
    public async Task UpdateUser_IncorrectUserID()
    {
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        validUser.Id = string.Empty;

        //Act
        var result = await userRepository.UpdateUserAsync(validUser);

        //Assert
        Assert.False(result.Succeeded);

        var isUserIDIsNullOrEmpty = result.ErrorExists("User ID cannot not be null or empty.");
        Assert.True(isUserIDIsNullOrEmpty);
    }

    #endregion

    #region User Models

    [Fact]
    public async Task GetUserExerciseRecords_ShouldReturnUserExerciseRecords()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var exerciseRepository = new ExerciseRepository(db);
        var muscleRepository = new MuscleRepository(db);
        var exercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Pull Up", ExerciseType.Reps, "Latissimus dorsi", "Biceps brachii", "Teres minor");

        var exerciseRecords = new[]
             {
                new ExerciseRecord()
                {
                    Date = DateTime.Now,
                    Reps = 20,
                    SumOfReps = 39,
                    CountOfTimes = 2,
                    UserId = user.Id,
                    ExerciseId = exercise.Id
                },
                new ExerciseRecord()
                {
                    Date = DateTime.Now.AddDays(-2),
                    Reps = 19,
                    SumOfReps = 19,
                    CountOfTimes = 1,
                    UserId = user.Id,
                    ExerciseId = exercise.Id
                }
            };

        var exerciseRecordRepository = new ExerciseRecordRepository(db);
        await exerciseRecordRepository.AddRangeAsync(exerciseRecords);

        //Act
        var userExerciseRecords = await userRepository.GetUserExerciseRecordsAsync(user.Id);

        //Assert
        Assert.NotNull(userExerciseRecords);
        Assert.NotEmpty(userExerciseRecords);
        Assert.Equal(exerciseRecords, userExerciseRecords);
    }

    [Fact]
    public async Task GetUserExerciseRecords_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        //Act
        var userExerciseRecords = await userRepository.GetUserExerciseRecordsAsync(user.Id);

        //Assert
        Assert.NotNull(userExerciseRecords);
        Assert.Empty(userExerciseRecords);
    }

    [Fact]
    public async Task GetUserMuscleSizes_ShouldReturnUserMuscleSizes()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        var muscleRepository = new MuscleRepository(db);
        var muscle = await MusclesInitializer.InitializeAsync(muscleRepository, new() { Name = "Back"}, null);

        var muscleSizes = new[]
             {
                new MuscleSize()
                {
                    Date = DateTime.Now.AddDays(-100),
                    Size = 35,
                    SizeType = SizeType.Centimeter,
                    MuscleId = muscle.Id,
                    UserId = user.Id
                }, 
                new MuscleSize()
                {
                    Date = DateTime.Now.AddDays(-30),
                    Size = 40,
                    SizeType = SizeType.Centimeter,
                    MuscleId = muscle.Id,
                    UserId = user.Id
                },
                new MuscleSize()
                {
                    Date = DateTime.Now,
                    Size = 42,
                    SizeType = SizeType.Centimeter,
                    MuscleId = muscle.Id,
                    UserId = user.Id
                }
            };

        var muscleSizeRepository = new MuscleSizeRepository(db);
        await muscleSizeRepository.AddRangeAsync(muscleSizes);

        //Act
        var userMuscleSizes = await userRepository.GetUserMuscleSizesAsync(user.Id);

        //Assert
        Assert.NotNull(userMuscleSizes);
        Assert.NotEmpty(userMuscleSizes);
        Assert.Equal(muscleSizes, userMuscleSizes);
    }

    [Fact]
    public async Task GetUserMuscleSizes_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        //Act
        var userMuscleSizes = await userRepository.GetUserMuscleSizesAsync(user.Id);

        //Assert
        Assert.NotNull(userMuscleSizes);
        Assert.Empty(userMuscleSizes);
    }

    [Fact]
    public async Task GetUserBodyWeights_ShouldReturnUserMuscleSizes()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        var bodyWeights = new[]
            {
                new BodyWeight()
                {
                    Date = DateTime.Now,
                    Weight = 70,
                    WeightType = WeightType.Kilogram,
                    UserId = user.Id
                },
                new BodyWeight()
                {
                    Date = DateTime.Now.AddDays(-2),
                    Weight = 62,
                    WeightType = WeightType.Pound,
                    UserId = user.Id
                }
            };

        var bodyWeightRepository = new BodyWeightRepository(db);
        await bodyWeightRepository.AddRangeAsync(bodyWeights);

        //Act
        var userBodyWeights = await userRepository.GetUserBodyWeightsAsync(user.Id);

        //Assert
        Assert.NotNull(userBodyWeights);
        Assert.NotEmpty(userBodyWeights);
        Assert.Equal(bodyWeights, userBodyWeights);
    }

    [Fact]
    public async Task GetUserBodyWeights_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        //Act
        var userBodyWeights = await userRepository.GetUserBodyWeightsAsync(user.Id);

        //Assert
        Assert.NotNull(userBodyWeights);
        Assert.Empty(userBodyWeights);
    }

    [Fact]
    public async Task GetUserWorkouts_ShouldReturnUserWorkouts()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        var muscleRepository = new MuscleRepository(db);
        var exerciseRepository = new ExerciseRepository(db);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var plankExercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
        var standingCalfRaisesExercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Standing Calf Raises", ExerciseType.WeightAndReps, "Gastrocnemius", "Soleus");
        var legRaiseExercise = await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Leg Raise", ExerciseType.Reps, "Rectus abdominis", "Hip flexors");

        var workouts = new[] {
            new Workout()
            {
                Name = "Abs",
                Exercises = new[] { plankExercise },
                UserId = user.Id
            },
            new Workout()
            {
                Name = "Abs and legs",
                Exercises = new[] { plankExercise, legRaiseExercise },
                UserId = user.Id
            },
            new Workout()
            {
                Name = "Legs",
                Exercises = new[] { standingCalfRaisesExercise, legRaiseExercise },
                UserId = user.Id
            }
        };

        var workoutRepository = new WorkoutRepository(db);
        await workoutRepository.AddRangeAsync(workouts);

        //Act
        var userWorkouts = await userRepository.GetUserWorkoutsAsync(user.Id);

        //Assert
        Assert.NotNull(userWorkouts);
        Assert.NotEmpty(userWorkouts);
        Assert.Equal(workouts, userWorkouts);
    }

    [Fact]
    public async Task GetUserWorkouts_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        //Act
        var userWorkouts = await userRepository.GetUserWorkoutsAsync(user.Id);

        //Assert
        Assert.NotNull(userWorkouts);
        Assert.Empty(userWorkouts);
    }

    [Fact]
    public async Task GetUserCreatedExercise_ShouldReturnUserCreatedExercise()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        await WorkoutContextFactory.InitializeMusclesAsync(db);

        var muscleRepository = new MuscleRepository(db);

        async Task<Exercise> CreateExerciseAsync(string name, ExerciseType exerciseType, params string[] muscleNames)
        {
            Exercise exercise = new();

            exercise.Name = name;
            exercise.Type = exerciseType;

            var muscles = new List<Muscle>();

            foreach (string muscleName in muscleNames)
            {
                var muscle = await muscleRepository!.GetByNameAsync(muscleName);
                if (muscle is not null)
                    muscles.Add(muscle);
            }

            exercise.WorkingMuscles = muscles;
            exercise.CreatedByUserId = user!.Id;

            return exercise;
        }

        var plankExercise = await CreateExerciseAsync("Plank", ExerciseType.Time, "Rectus abdominis", "External oblique", "Quadriceps");
        var pullUpExercise = await CreateExerciseAsync("Pull Up", ExerciseType.Reps, "Latissimus dorsi", "Biceps brachii", "Teres minor");
        var pushUpExercise = await CreateExerciseAsync("Push Up", ExerciseType.Reps, "Pectoralis major", "Triceps brachii", "Deltoids");
        var exercises = new[] { plankExercise, pullUpExercise, pushUpExercise };

        var exerciseRepository = new ExerciseRepository(db);
        await exerciseRepository.AddRangeAsync(exercises);

        //Act
        var userExercises = await userRepository.GetUserCreatedExercisesAsync(user.Id);

        //Assert
        Assert.NotNull(userExercises);
        Assert.NotEmpty(userExercises);
        Assert.Equal(exercises, userExercises);
    }

    [Fact]
    public async Task GetUserCreatedExercise_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        await userRepository.AddUserAsync(user);

        //Act
        var userCreatedExercise = await userRepository.GetUserCreatedExercisesAsync(user.Id);

        //Assert
        Assert.NotNull(userCreatedExercise);
        Assert.Empty(userCreatedExercise);
    }

    #endregion

    #region Password

    [Fact]
    public async Task ChangeUserPassword_ShouldChangeUserPasswordSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        string password = "P@$$w0rd";
        await userRepository.CreateUserAsync(validUser, password);

        string newPassword = "Newre4e4r6g4erP@$$w0rd";

        //Act
        var result = await userRepository.ChangeUserPasswordAsync(validUser.Id, password, newPassword);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task ChangeUserPassword_UserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        string password = "P@$$w0rd";

        //Act
        string newPassword = "New_P@$$w0rd";
        var result = await userRepository.ChangeUserPasswordAsync(validUser.Id, password, newPassword);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);

        var isUserNotFound = result.ErrorExists("User not found.");
        Assert.True(isUserNotFound);
    }

    [Fact]
    public async Task AddUserPassword_ShouldAddUserPasswordSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        string password = "P@$$w0rd";

        //Act
        var result = await userRepository.AddUserPasswordAsync(validUser.Id, password);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AddUserPassword_UserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        string password = "P@$$w0rd";

        //Act
        var result = await userRepository.AddUserPasswordAsync(validUser.Id, password);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);

        var isUserNotFound = result.ErrorExists("User not found.");
        Assert.True(isUserNotFound);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldReturnTrue()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        string password = "P@$$w0rd";
        await userRepository.CreateUserAsync(validUser, password);

        //Act
        var result = await userRepository.HasUserPasswordAsync(validUser.Id);

        //Assert
        Assert.True(result);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldReturnFalse()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var validUser = GetValidUser();
        await userRepository.AddUserAsync(validUser);

        //Act
        var result = await userRepository.HasUserPasswordAsync(validUser.Id);

        //Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HasUserPasswordAsync_ShouldThrowException_UserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var notFoundUserID = Guid.NewGuid().ToString();

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await userRepository.HasUserPasswordAsync(notFoundUserID));
        Assert.Equal("User not found.", ex.Message);
    }

    #endregion

    #region Roles

    [Fact]
    public async Task GetUserRoles_ShouldReturnUserRoles()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var user = GetValidUser();
        string password = "P@$$w0rd";
        await userRepository.CreateUserAsync(user, password);

        var roles = new[] { Roles.UserRole, Roles.AdminRole };
        await userRepository.AddRolesToUserAsync(user.Id, roles);

        //Act
        var userRoles = await userRepository.GetUserRolesAsync(user.Id);

        //Assert
        Assert.NotNull(userRoles);
        Assert.NotEmpty(userRoles);
        Assert.Equal(roles.Count(), userRoles.Count());
    }

    [Fact]
    public async Task GetUserRoles_ShouldReturnEmpty()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        var user = GetValidUser();
        string password = "P@$$w0rd";
        await userRepository.CreateUserAsync(user, password);

        //Act
        var userRoles = await userRepository.GetUserRolesAsync(user.Id);

        //Assert
        Assert.NotNull(userRoles);
        Assert.Empty(userRoles);
    }

    [Fact]
    public async Task GetUserRoles_ShouldThrowException()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        //Act & Assert
        var ex = await Assert.ThrowsAsync<NotFoundException>(async () => await userRepository.GetUserRolesAsync(Guid.NewGuid().ToString()));
        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task AddRolesToUser_ShouldAddRolesToUserSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var user = GetValidUser();
        string password = "P@$$w0rd";
        await userRepository.CreateUserAsync(user, password);

        var roles = new[] { Roles.UserRole, Roles.AdminRole };

        //Act
        var result = await userRepository.AddRolesToUserAsync(user.Id, roles);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);

        var userRoles = await userRepository.GetUserRolesAsync(user.Id);
        Assert.NotNull(userRoles);
        Assert.Equal(userRoles, roles);
    }

    [Fact]
    public async Task AddRolesToUser_UserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        await WorkoutContextFactory.InitializeRolesAsync(db);
        var roles = new[] { Roles.UserRole, Roles.AdminRole };

        //Act
        var result = await userRepository.AddRolesToUserAsync(Guid.NewGuid().ToString(), roles);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);

        var isUserNotFound = result.ErrorExists("User not found.");
        Assert.True(isUserNotFound);
    }

    [Fact]
    public async Task DeleteRoleFromUser_ShouldDeleteRoleFromUserSuccessfully()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        var user = GetValidUser();
        string password = "P@$$w0rd";
        await userRepository.CreateUserAsync(user, password);

        var roles = new[] { Roles.UserRole, Roles.AdminRole };
        await userRepository.AddRolesToUserAsync(user.Id, roles);

        //Act
        var result = await userRepository.DeleteRoleFromUserAsync(user.Id, Roles.UserRole);

        //Assert
        Assert.NotNull(result);
        Assert.True(result.Succeeded);

        var userRoles = await userRepository.GetUserRolesAsync(user.Id);
        Assert.NotNull(userRoles);
        Assert.Single(userRoles);
        Assert.Equal(Roles.AdminRole, userRoles.First());
    }

    [Fact]
    public async Task DeleteRoleFromUser_UserNotFound()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        await WorkoutContextFactory.InitializeRolesAsync(db);

        //Act
        var result = await userRepository.DeleteRoleFromUserAsync(Guid.NewGuid().ToString(), Roles.UserRole);

        //Assert
        Assert.NotNull(result);
        Assert.False(result.Succeeded);

        var isUserNotFound = result.ErrorExists("User not found.");
        Assert.True(isUserNotFound);
    }

    #endregion
}
