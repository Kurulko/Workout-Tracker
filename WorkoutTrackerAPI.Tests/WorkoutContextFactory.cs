using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using Newtonsoft.Json.Linq;
using System.Data;

namespace WorkoutTrackerAPI.Tests;

public class WorkoutContextFactory
{
    public WorkoutDbContext CreateDatabaseContext()
    {
        var options = new DbContextOptionsBuilder<WorkoutDbContext>()
               .UseInMemoryDatabase(Guid.NewGuid().ToString())
               .Options;

        return new WorkoutDbContext(options);
    }

    public static async Task InitializeRolesAsync(WorkoutDbContext db)
    {
        var roleManager = IdentityHelper.GetRoleManager(db);
        var roleRepository = new RoleRepository(roleManager);

        var roles = await roleRepository.GetRolesAsync();
        if (!roles.Any())
            await RolesInitializer.InitializeAsync(roleRepository, Roles.AdminRole, Roles.UserRole);
    }

    public static async Task InitializeMusclesAsync(WorkoutDbContext db)
    {
        var muscleRepository = new MuscleRepository(db);

        var muscles = await muscleRepository.GetAllAsync();
        if (!muscles.Any())
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
