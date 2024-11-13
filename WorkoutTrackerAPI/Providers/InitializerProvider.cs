using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Services.MuscleServices;
using WorkoutTrackerAPI.Services.RoleServices;
using WorkoutTrackerAPI.Services.UserServices;

namespace WorkoutTrackerAPI.Providers;

public static class InitializerProvider
{
    public static async Task InitializeDataAsync(this WebApplication app, ConfigurationManager config)
    {
        using IServiceScope serviceScope = app.Services.CreateScope();

        IServiceProvider serviceProvider = serviceScope.ServiceProvider;

        var roleRepository = serviceProvider.GetRequiredService<RoleRepository>();
        if (!(await roleRepository.AnyAsync()))
            await InitializeRolesAsync(roleRepository);

        var userRepository = serviceProvider.GetRequiredService<UserRepository>();
        if (!(await userRepository.AnyUsersAsync()))
            await InitializeUsersAsync(userRepository, config);

        var muscleRepository = serviceProvider.GetRequiredService<MuscleRepository>();
        if(!(await muscleRepository.AnyAsync()))
            await InitializeMusclesAsync(muscleRepository);

        var equipmentRepository = serviceProvider.GetRequiredService<EquipmentRepository>();
        if (!(await equipmentRepository.AnyAsync()))
            await InitializeEquipmentsAsync(equipmentRepository);

        var exerciseRepository = serviceProvider.GetRequiredService<ExerciseRepository>();
        if (!(await exerciseRepository.AnyAsync()))
            await InitializeExercisesAsync(exerciseRepository, muscleRepository);
    }

    static async Task InitializeRolesAsync(RoleRepository roleRepository)
        => await RolesInitializer.InitializeAsync(roleRepository, Roles.AdminRole, Roles.UserRole);

    static async Task InitializeUsersAsync(UserRepository userRepository, ConfigurationManager config)
    {
        string adminName = config.GetValue<string>("Admin:Name")!;
        string adminPassword = config.GetValue<string>("Admin:Password")!;
        string adminEmail = config.GetValue<string>("Admin:Email")!;

        await UsersInitializer.InitializeAsync(userRepository, adminName, adminEmail, adminPassword, new[] { Roles.AdminRole, Roles.UserRole });
    }

    static async Task InitializeMusclesAsync(MuscleRepository muscleRepository)
    {
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
    
    static async Task InitializeEquipmentsAsync(EquipmentRepository equipmentRepository)
    {
        string json = await File.ReadAllTextAsync("Data/Source/equipments.json");

        // Parse the JSON
        var jsonObject = JObject.Parse(json);

        // Get the "Muscles" property as a JArray
        var equipmentArray = (JArray)jsonObject["Equipments"]!;

        // Deserialize the "Muscles" array into a list
        var equipmentNames = equipmentArray.ToObject<List<string>>()!;

        foreach (var equipmentName in equipmentNames)
            await EquipmentInitializer.InitializeAsync(equipmentRepository, equipmentName);
    }

    static async Task InitializeExercisesAsync(ExerciseRepository exerciseRepository, MuscleRepository muscleRepository)
    {
        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Plank", ExerciseType.Time, 
            "Rectus abdominis", "External oblique", "Quadriceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Pull Up", ExerciseType.Reps, 
            "Latissimus dorsi", "Biceps brachii", "Teres minor");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Push Up", ExerciseType.Reps,
            "Pectoralis major", "Triceps", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Bench Press", ExerciseType.WeightAndReps,
            "Pectoralis major", "Triceps", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Squat", ExerciseType.WeightAndReps,
            "Quadriceps", "Gluteus maximus", "Hamstrings");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Deadlift", ExerciseType.WeightAndReps,
            "Erector spinae", "Gluteus maximus", "Hamstrings", "Quadriceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Shoulder Press", ExerciseType.WeightAndReps,
            "Deltoids", "Triceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Lunges", ExerciseType.WeightAndReps,
            "Quadriceps", "Gluteus maximus", "Hamstrings", "Adductors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Dips", ExerciseType.Reps,
            "Triceps", "Pectoralis major", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Leg Raise", ExerciseType.Reps,
            "Rectus abdominis", "Hip flexors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Crunches", ExerciseType.Reps,
        "Rectus abdominis");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Shrugs", ExerciseType.WeightAndReps,
            "Trapezius");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Biceps Curl", ExerciseType.WeightAndReps,
            "Biceps brachii");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Standing Calf Raises", ExerciseType.WeightAndReps, "Gastrocnemius", "Soleus");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Dumbbell Chest Fly", ExerciseType.WeightAndReps, "Pectoralis major", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Seated Overhead Extensions", ExerciseType.WeightAndReps,  "Triceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Leg Extension", ExerciseType.WeightAndReps,
            "Quadriceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Single Arm Dumbbell Rows", ExerciseType.WeightAndReps, "Latissimus dorsi", "Biceps brachii", "Rhomboids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Dumbbell Squeeze Press", ExerciseType.WeightAndReps, "Pectoralis major", "Triceps", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Seated Dumbbell Wrist Curl", ExerciseType.WeightAndReps, "Forearms");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Narrow Grip Preacher Curl", ExerciseType.WeightAndReps, "Biceps brachii");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Barbell Deadlift", ExerciseType.WeightAndReps, "Erector spinae", "Gluteus maximus", "Hamstrings", "Quadriceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Bent Over Row", ExerciseType.WeightAndReps,
            "Latissimus dorsi", "Rhomboids", "Biceps brachii");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Barbell Back Squat", ExerciseType.WeightAndReps, "Quadriceps", "Gluteus maximus", "Hamstrings");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Weighted Plank", ExerciseType.Time,
            "Rectus abdominis", "External oblique", "Quadriceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Weighted Russian Twists", ExerciseType.Reps,
            "External oblique", "Rectus abdominis");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Weighted V-ups", ExerciseType.Reps,
            "Rectus abdominis", "Hip flexors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Ab Rollouts", ExerciseType.Reps,
            "Rectus abdominis", "External oblique", "Hip flexors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Barbell Sumo Squat", ExerciseType.WeightAndReps, "Quadriceps", "Adductors", "Gluteus maximus");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Weighted Russian Twists (Legs Up)", ExerciseType.Reps, "External oblique", "Rectus abdominis", "Hip flexors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Clap Push-Ups", ExerciseType.Reps,
        "Pectoralis major", "Triceps", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Renegade Row Push-Ups", ExerciseType.WeightAndReps, "Latissimus dorsi", "Biceps brachii", "Pectoralis major", "Triceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Weighted Push-Ups", ExerciseType.WeightAndReps, "Pectoralis major", "Triceps", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Elevated Push-Ups", ExerciseType.Reps,
            "Pectoralis major", "Triceps", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Weighted Pull-Ups", ExerciseType.WeightAndReps, "Latissimus dorsi", "Biceps brachii");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Wide Grip Pull-Ups", ExerciseType.Reps,
            "Latissimus dorsi", "Teres major", "Biceps brachii");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Uneven Push-Ups on a Ball", ExerciseType.Reps, "Pectoralis major", "Triceps", "Deltoids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Weighted Side Plank", ExerciseType.Time,
            "External oblique", "Core", "Gluteus medius");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Upright Row", ExerciseType.WeightAndReps,
            "Deltoids", "Trapezius", "Biceps brachii");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Seated In-and-Out", ExerciseType.Reps,
            "Rectus abdominis", "Hip flexors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Toes to Bar", ExerciseType.Reps,
            "Rectus abdominis", "Hip flexors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Weighted Hanging Knee Raises", ExerciseType.WeightAndReps, "Rectus abdominis", "Hip flexors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Front Squat", ExerciseType.WeightAndReps,
            "Quadriceps", "Gluteus maximus", "Hamstrings");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Standing Side Bend", ExerciseType.Reps,
            "External oblique");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Crossbody Hammer Curl", ExerciseType.WeightAndReps, "Biceps brachii", "Brachialis");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Band Bicycle Crunch", ExerciseType.Reps,
            "Rectus abdominis", "External oblique");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Hollow Body Hold", ExerciseType.Time,
            "Rectus abdominis", "External oblique", "Abs");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Bent-Over Reverse Fly", ExerciseType.WeightAndReps, "Deltoids", "Rhomboids");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Seated Overhead Press", ExerciseType.WeightAndReps, "Deltoids", "Triceps");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Reverse Dumbbell Wrist Curl", ExerciseType.WeightAndReps, "Forearm extensors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Jogging", ExerciseType.Time,
            "Cardiovascular", "Leg muscles");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Lying Weighted Neck Flexion", ExerciseType.WeightAndReps, "Neck flexors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Lying Weighted Neck Extension", ExerciseType.WeightAndReps, "Neck extensors");

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, "Plate Front Raise", ExerciseType.WeightAndReps, "Deltoids", "Trapezius");
    }
}