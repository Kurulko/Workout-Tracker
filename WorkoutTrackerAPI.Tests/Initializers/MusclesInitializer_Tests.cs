﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data.Models.UserModels;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories;
using WorkoutTracker.API.Services;
using WorkoutTracker.API.Services.MuscleServices;
using WorkoutTracker.API.Services.RoleServices;
using Xunit;

namespace WorkoutTracker.API.Tests.Initializers;

public class MusclesInitializer_Tests
{
    [Fact]
    public static async Task InitializeAsync()
    {
        //Arrange
        using var db = WorkoutContextFactory.CreateDatabaseContext();
        var muscleRepository = new MuscleRepository(db);

        string json = await File.ReadAllTextAsync("Data/Source/muscles.json");
        var jsonObject = JObject.Parse(json);
        var musclesArray = (JArray)jsonObject["Muscles"]!;
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

