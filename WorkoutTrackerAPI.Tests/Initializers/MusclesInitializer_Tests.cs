using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.MuscleServices;
using WorkoutTrackerAPI.Services.RoleServices;
using Xunit;

namespace WorkoutTrackerAPI.Tests.Initializers;

public class MusclesInitializer_Tests
{
    [Fact]
    public static async Task InitializeAsync()
    {
        //Arrange
        WorkoutContextFactory factory = new();
        using var db = factory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);
        //IMuscleService muscleService = new MuscleService(baseWorkoutRepository);

        string json = await File.ReadAllTextAsync("Data/Source/muscles.json");

        // Parse the JSON
        var jsonObject = JObject.Parse(json);

        // Get the "Muscles" property as a JArray
        var musclesArray = (JArray)jsonObject["Muscles"]!;

        // Deserialize the "Muscles" array into a list
        var muscleData = musclesArray.ToObject<List<MuscleData>>()!;

        //Act
        foreach (var muscle in muscleData)
            await MusclesInitializer.InitializeAsync(muscleRepository, muscle, null);

        //Assert
        string neckMuscleStr = "Neck";
        Muscle? neckMuscle = await muscleRepository.GetByNameAsync(neckMuscleStr);

        Assert.NotNull(neckMuscle);
        Assert.Equal(neckMuscle?.Name, neckMuscleStr);

        string trapeziusMuscleStr = "Trapezius";
        Muscle? trapeziusMuscle = await muscleRepository.GetByNameAsync(trapeziusMuscleStr);

        Assert.NotNull(trapeziusMuscle);
        Assert.Equal(trapeziusMuscle?.Name, trapeziusMuscleStr);

        var allMuscles = await muscleRepository.GetAllAsync();
        Assert.NotNull(allMuscles);
        Assert.Contains(neckMuscle, allMuscles!);
        Assert.Contains(trapeziusMuscle, allMuscles!);
    }
}

