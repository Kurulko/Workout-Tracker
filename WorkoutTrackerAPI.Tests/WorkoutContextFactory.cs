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
               .UseInMemoryDatabase(Guid.NewGuid().ToString())
               .Options;

        return new WorkoutDbContext(options); ;
    }

    public static async Task InitializeRolesAsync(WorkoutDbContext db)
    {
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var roles = await roleRepository.GetRolesAsync();
        if (roles.Count() == 0)
            await RolesInitializer.InitializeAsync(roleRepository, Roles.AdminRole, Roles.UserRole);
    }

    public static async Task InitializeMusclesAsync(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);

        var muscles = await muscleRepository.GetAllAsync();
        if (muscles.Count() == 0)
        {
            string json = await File.ReadAllTextAsync("Data/Source/muscles.json");
            var jsonObject = JObject.Parse(json);
            var musclesArray = (JArray)jsonObject["Muscles"]!;
            var muscleData = musclesArray.ToObject<List<MuscleData>>()!;

            foreach (var muscle in muscleData)
                await MusclesInitializer.InitializeAsync(muscleRepository, muscle, null);
        }
    }
}
