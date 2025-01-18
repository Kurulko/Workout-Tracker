using Newtonsoft.Json.Linq;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;
using WorkoutTrackerAPI.Services.FileServices;

namespace WorkoutTrackerAPI.Providers;

public static class InitializerProvider
{
    const string sourceFolderPath = "Data/Source";

    public static async Task InitializeDataAsync(this WebApplication app, ConfigurationManager config)
    {
        using IServiceScope serviceScope = app.Services.CreateScope();

        IServiceProvider serviceProvider = serviceScope.ServiceProvider;

        var fileService = serviceProvider.GetRequiredService<IFileService>();

        var roleRepository = serviceProvider.GetRequiredService<RoleRepository>();
        if (!await roleRepository.AnyAsync())
            await InitializeRolesAsync(roleRepository);

        var userRepository = serviceProvider.GetRequiredService<UserRepository>();
        if (!await userRepository.AnyUsersAsync())
            await InitializeUsersAsync(userRepository, config);

        var muscleRepository = serviceProvider.GetRequiredService<MuscleRepository>();
        if(!await muscleRepository.AnyAsync())
            await InitializeMusclesAsync(muscleRepository);

        var equipmentRepository = serviceProvider.GetRequiredService<EquipmentRepository>();
        if (!await equipmentRepository.AnyAsync())
            await InitializeEquipmentsAsync(equipmentRepository);

        var exerciseRepository = serviceProvider.GetRequiredService<ExerciseRepository>();
        if (!await exerciseRepository.AnyAsync())
            await InitializeExercisesAsync(exerciseRepository, muscleRepository, equipmentRepository);
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
        string muscleNamesFilePath = Path.Combine(sourceFolderPath, "muscle-names.json");
        string json = await File.ReadAllTextAsync(muscleNamesFilePath);

        var jsonObject = JObject.Parse(json);
        var musclesArray = (JArray)jsonObject["Muscles"]!;
        var muscleData = musclesArray.ToObject<List<MuscleData>>()!;

        foreach (var muscle in muscleData)
            await MusclesInitializer.InitializeAsync(muscleRepository, muscle, null);
    }
    
    static async Task InitializeEquipmentsAsync(EquipmentRepository equipmentRepository)
    {
        string equipmentNamesFilePath = Path.Combine(sourceFolderPath, "equipment-names.json");
        string json = await File.ReadAllTextAsync(equipmentNamesFilePath);

        var jsonObject = JObject.Parse(json);
        var equipmentArray = (JArray)jsonObject["Equipments"]!;
        var equipmentNames = equipmentArray.ToObject<List<string>>()!;

        foreach (var equipmentName in equipmentNames)
            await EquipmentInitializer.InitializeAsync(equipmentRepository, equipmentName);
    }

    static async Task InitializeExercisesAsync(ExerciseRepository exerciseRepository, MuscleRepository muscleRepository, EquipmentRepository equipmentRepository)
    {
        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Pull-ups", GetExercisePhoto("wide_grip_pull_ups.png"), new[] { "Pull-Up Bar" }, ExerciseType.Reps, new[] { "Latissimus dorsi", "Biceps brachii short head", "Biceps brachii long head" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Push-ups", GetExercisePhoto("push_ups.jpg"), new[] { "No Equipment" }, ExerciseType.Reps, new[] { "Pectoralis major", "Triceps" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Bench Press", GetExercisePhoto("bench_press.jpg"), new[] { "Barbells" }, ExerciseType.WeightAndReps, new[] { "Pectoralis major", "Triceps", "Deltoid anterior" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Deadlifts", GetExercisePhoto("barbell_deadlift.jpg"), new[] { "Barbells" }, ExerciseType.WeightAndReps, new[] { "Gluteus maximus", "Hamstrings", "Erector spinae" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Crunches", GetExercisePhoto("do_abdominal_crunches.png"), new[] { "No Equipment" }, ExerciseType.Reps, new[] { "Rectus abdominis" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Shrugs", GetExercisePhoto("shurgs.jpg"), new[] { "Barbells" }, ExerciseType.WeightAndReps, new[] { "Trapezius" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Biceps Curl", GetExercisePhoto("biceps_curl.png"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Biceps brachii short head", "Biceps brachii long head" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Standing Calf Raises", GetExercisePhoto("standing_calf_raises.jpg"), new[] { "No Equipment" }, ExerciseType.Reps, new[] { "Gastrocnemius", "Soleus" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Dumbbell Chest Fly", GetExercisePhoto("dumbbell_chest_fly.jpg"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Pectoralis major" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Seated Overhead Extensions", GetExercisePhoto("seated_dumbbell_overhead_extensions.jpg"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Triceps" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Leg Extension", GetExercisePhoto("leg_extension.png"), new[] { "Leg Extension Machine" }, ExerciseType.WeightAndReps, new[] { "Quadriceps" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Single Arm Dumbbell Rows", GetExercisePhoto("single_arm_dumbbell_rows.jpg"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Latissimus dorsi", "Rhomboid major" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Dumbbell Squeeze Press", GetExercisePhoto("dumbbell_squeeze_bench_press.png"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Pectoralis major", "Triceps" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Seated Dumbbell Wrist Curl", GetExercisePhoto("seated_dumbbell_wrist_curl.png"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Forearms" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Narrow Grip Preacher Curl", GetExercisePhoto("barbell_preacher_curl.png"), new[] { "Preacher Curl Bench" }, ExerciseType.WeightAndReps, new[] { "Biceps brachii short head", "Biceps brachii long head" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Bent-over Row", GetExercisePhoto("bent_over_row.jpg"), new[] { "Barbells" }, ExerciseType.WeightAndReps, new[] { "Latissimus dorsi", "Rhomboid major" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Barbell Back Squat", GetExercisePhoto("barbell_back_squat.jpg"), new[] { "Barbells" }, ExerciseType.WeightAndReps, new[] { "Quadriceps", "Gluteus maximus", "Hamstrings" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Plank", GetExercisePhoto("weighted_ plank.jpg"), new[] { "Weighted Vest" }, ExerciseType.WeightAndTime, new[] { "Rectus abdominis", "External oblique" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Russian Twists", GetExercisePhoto("weighted_russian_twists.png"), new[] { "Weighted Vest" }, ExerciseType.Reps, new[] { "External oblique" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted V-ups", GetExercisePhoto("weighted_v_ups.png"), new[] { "Weighted Vest" }, ExerciseType.Reps, new[] { "Rectus abdominis", "Hip flexors" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Ab Rollouts", GetExercisePhoto("ab_rollouts.png"), new[] { "Core Wheels" }, ExerciseType.Reps, new[] { "Rectus abdominis", "Hip flexors" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Barbell Sumo Squat", GetExercisePhoto("sumo_squat.png"), new[] { "Barbells" }, ExerciseType.WeightAndReps, new[] { "Quadriceps", "Gluteus maximus", "Adductors" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Russian Twists (Legs Up)", GetExercisePhoto("weighted_russian_twists_legs_up.png"), new[] { "Weighted Vest" }, ExerciseType.Reps, new[] { "External oblique", "Rectus abdominis" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Clap Push-ups", GetExercisePhoto("clap_push_ups.png"), new[] { "No Equipment" }, ExerciseType.Reps, new[] { "Pectoralis major", "Triceps", "Deltoids" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Renegade Row Push-ups", GetExercisePhoto("dumbbell_renegade_row_push_ups.png"), new[] { "No Equipment" }, ExerciseType.Reps, new[] { "Latissimus dorsi", "Triceps", "Core" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Push-ups", GetExercisePhoto("weighted_push_ups.png"), new[] { "Weighted Vest" }, ExerciseType.Reps, new[] { "Pectoralis major", "Triceps" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Elevated Push-ups", GetExercisePhoto("elevated_push_ups.png"), new[] { "No Equipment" }, ExerciseType.Reps, new[] { "Pectoralis major", "Triceps" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Pull-ups", GetExercisePhoto("weighted_pull_ups.png"), new[] { "Pull-Up Bar" }, ExerciseType.Reps, new[] { "Latissimus dorsi", "Biceps brachii short head", "Biceps brachii long head" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Wide Grip Pull-ups", GetExercisePhoto("wide_grip_pull_ups.png"), new[] { "Pull-Up Bar" }, ExerciseType.Reps, new[] { "Latissimus dorsi", "Biceps brachii short head", "Biceps brachii long head" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Uneven Push-ups on a Ball", GetExercisePhoto("uneven_push_ups_on_a_ball.png"), new[] { "No Equipment" }, ExerciseType.Reps, new[] { "Pectoralis major", "Triceps" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Side Plank", GetExercisePhoto("side_plank.png"), new[] { "Weighted Vest" }, ExerciseType.WeightAndTime, new[] { "External oblique" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Upright Row", GetExercisePhoto("upright_row.png"), new[] { "Barbells" }, ExerciseType.WeightAndReps, new[] { "Deltoids", "Trapezius" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Seated In-and-Out", GetExercisePhoto("seated_in-and-out.png"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Core" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Toes to Bar", GetExercisePhoto("toes_to_bar.jpg"), new[] { "Pull-Up Bar" }, ExerciseType.Reps, new[] { "Rectus abdominis", "Hip flexors" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Hanging Knee Raises", GetExercisePhoto("weighted_hanging_knee_raises.png"), new[] { "Weight Plates" }, ExerciseType.WeightAndReps, new[] { "Rectus abdominis", "Hip flexors" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Front Squat", GetExercisePhoto("front_squat.png"), new[] { "Barbells" }, ExerciseType.WeightAndReps, new[] { "Quadriceps", "Gluteus maximus" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Standing Side Bend", GetExercisePhoto("standing_side_bend.png"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "External oblique" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Crossbody Hammer Curl", GetExercisePhoto("crossbody_hammer_curl.png"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Biceps brachii short head", "Biceps brachii long head" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Band Bicycle Crunch", GetExercisePhoto("band_bicycle_crunch.png"), new[] { "Resistance Bands" }, ExerciseType.Reps, new[] { "Rectus abdominis", "External oblique" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Hollow Body Hold", GetExercisePhoto("hollow_body_hold.jpg"), new[] { "No Equipment" }, ExerciseType.Time, new[] { "Rectus abdominis" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Bent-Over Reverse Fly", GetExercisePhoto("bent-over_reverse_fly.png"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Deltoids", "Rhomboid major" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Seated Overhead Press", GetExercisePhoto("seated_overhead_press.png"), new[] { "Barbells" }, ExerciseType.WeightAndReps, new[] { "Deltoids", "Triceps" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Reverse Dumbbell Wrist Curl", GetExercisePhoto("reverse_dumbbell_wrist_curl.png"), new[] { "Dumbbells" }, ExerciseType.WeightAndReps, new[] { "Forearms" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Jogging", GetExercisePhoto("jogging_2.png"), new[] { "Treadmill" }, ExerciseType.Time, new[] { "Core", "Legs" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Lying Weighted Neck Flexion", GetExercisePhoto("lying_weighted_neck_flexion.jpg"), new[] { "Weight Plates" }, ExerciseType.WeightAndReps, new[] { "Sternocleidomastoid" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Lying Weighted Neck Extension", GetExercisePhoto("lying_weighted_neck_extension.png"), new[] { "Weight Plates" }, ExerciseType.WeightAndReps, new[] { "Trapezius" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Plate Front Raise", GetExercisePhoto("plate_front_raise.png"), new[] { "Weight Plates" }, ExerciseType.WeightAndReps, new[] { "Deltoids" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Hanging Leg Raise", GetExercisePhoto("hanging_leg_raises.png"), new[] { "No Equipment" },
            ExerciseType.WeightAndReps, new[] { "Abs" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Sumo Deadlift", GetExercisePhoto("sumo_deadlift.png"), new[] { "Barbells" },
            ExerciseType.WeightAndReps, new[] { "Hamstrings", "Glutes" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Dips", GetExercisePhoto("dips.png"), new[] { "Dip Bars" },
            ExerciseType.WeightAndReps, new[] { "Triceps", "Chest" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Bench Dips", GetExercisePhoto("bench_dips.png"), new[] { "Bench" },
            ExerciseType.WeightAndReps, new[] { "Triceps", "Chest" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Hammers", GetExercisePhoto("hammers.png"), new[] { "Dumbbells" },
            ExerciseType.WeightAndReps, new[] { "Forearms" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Incline Bench Press", GetExercisePhoto("incline_bench_press.png"), new[] { "Barbells", "Bench" },
            ExerciseType.WeightAndReps, new[] { "Chest", "Deltoids" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Triceps Pushdown", GetExercisePhoto("triceps_pushdown.png"), new[] { "Cable Machine" },
            ExerciseType.WeightAndReps, new[] { "Triceps" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Neutral Grip Lat Pulldown", GetExercisePhoto("neutral_grip_lat_pulldown.png"), new[] { "Cable Machine" },
            ExerciseType.WeightAndReps, new[] { "Latissimus Dorsi" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Burpees", GetExercisePhoto("burpee.png"), new[] { "No Equipment" },
            ExerciseType.Reps, new[] { "Core", "Legs" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Cable Crunches", GetExercisePhoto("cable_crunches.png"), new[] { "Cable Machine" },
            ExerciseType.WeightAndReps, new[] { "Abs" });

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Lunges", GetExercisePhoto("lunges.png"), new[] { "Dumbbells" },
            ExerciseType.WeightAndReps, new[] { "Quadriceps", "Glutes", "Hamstrings" });

    }

    static string GetExercisePhoto(string fileName)
    {
        string exercisePhotosPath = Path.Combine("photos", "exercises");
        return Path.Combine(exercisePhotosPath, fileName);
    }
}