using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Services.RoleServices;
using WorkoutTrackerAPI.Services.UserServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WorkoutTrackerAPI.Services;
using Microsoft.AspNetCore.Http;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Providers;
using WorkoutTrackerAPI.Services.MuscleServices;
using WorkoutTrackerAPI.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Repositories.UserRepositories;

namespace WorkoutTrackerAPI.Tests;

public class WorkoutContextFactory
{
    public WorkoutDbContext CreateDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<WorkoutDbContext>()
               //.UseInMemoryDatabase(Guid.NewGuid().ToString())
               .UseInMemoryDatabase("Workout")
               .Options;

        var db = new WorkoutDbContext(options);

        db.Database.EnsureCreated();
        db.Database.EnsureDeleted();

        return db;
        //return new WorkoutDbContext(options);
    }

    static internal async Task InitializeDefaultExercisesAsync(WorkoutDbContext db)
    {
        await InitializeMusclesAsync(db);

        var exerciseRepository = new ExerciseRepository(db);
        var muscleRepository = new MuscleRepository(db);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Plank", ExerciseType.Time,
            "Rectus abdominis", "External oblique", "Quadriceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Pull Up", ExerciseType.Reps,
            "Latissimus dorsi", "Biceps brachii", "Teres minor");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Push Up", ExerciseType.Reps,
            "Pectoralis major", "Triceps brachii", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Bench Press", ExerciseType.WeightAndReps,
            "Pectoralis major", "Triceps brachii", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Squat", ExerciseType.WeightAndReps,
            "Quadriceps", "Gluteus maximus", "Hamstrings");
    }

    static internal async Task InitializeMusclesAsync(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);

        string json = await File.ReadAllTextAsync("Data/Source/muscles.json");

        // Parse the JSON
        var jsonObject = JObject.Parse(json);

        // Get the "Muscles" property as a JArray
        var musclesArray = (JArray)jsonObject["Muscles"]!;

        // Deserialize the "Muscles" array into a list
        var muscleData = musclesArray.ToObject<List<MuscleData>>()!;

        foreach (var muscle in muscleData)
            await MusclesInitializer.InitializeAsync(muscleRepository, muscle, null);
    }

    static internal async Task InitializeRolesAsync(WorkoutDbContext db)
    {
        // create a RoleManager instance
        var roleManager = IdentityHelper.GetRoleManager(db);

        var roleRepository = new RoleRepository(roleManager);
        await RolesInitializer.InitializeAsync(roleRepository, Roles.AdminRole, Roles.UserRole);
    }

    static internal async Task InitializeDefaultUsersAsync(WorkoutDbContext db)
    {
        await InitializeRolesAsync(db);

        // create a UserManager instance
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        // create a IConfiguration mock instance
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "DefaultPasswords:User")]).Returns("P@$$w0rd");
        mockConfiguration.SetupGet(x => x[It.Is<string>(s => s == "DefaultPasswords:Admin")]).Returns("P@$$w0rd");

        // setup the default role names
        string name_User = "User";
        string name_Admin = "Admin";

        string email_User = "user@email.com";
        string email_Admin = "admin@email.com";

        string password_User = mockConfiguration.Object["DefaultPasswords:User"]!;
        string password_Admin = mockConfiguration.Object["DefaultPasswords:Admin"]!;

        await UsersInitializer.InitializeAsync(userRepository, name_User, email_User, password_User, Roles.UserRole);
        await UsersInitializer.InitializeAsync(userRepository, name_Admin, email_Admin, password_User, Roles.UserRole, Roles.AdminRole);
    }
}
