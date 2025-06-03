using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using WorkoutTracker.Application.Common.Settings;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Initializers;
using WorkoutTracker.Infrastructure.Models;

namespace WorkoutTracker.Infrastructure.Providers;

public static class InitializerProvider
{
    public static async Task InitializeDataAsync(this WebApplication app, IConfiguration config, CancellationToken cancellationToken = default)
    {
        using IServiceScope serviceScope = app.Services.CreateScope();

        IServiceProvider serviceProvider = serviceScope.ServiceProvider;

        var seedDataOptions = serviceProvider.GetRequiredService<SeedDataOptions>();

        var roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
        var anyRoles = await roleRepository.AnyAsync();
        if (!anyRoles)
            await InitializeRolesAsync(roleRepository);

        var muscleRepository = serviceProvider.GetRequiredService<IMuscleRepository>();
        var anyMuscles = await muscleRepository.AnyAsync(cancellationToken);
        if (!anyMuscles)
            await InitializeMusclesAsync(muscleRepository, seedDataOptions, cancellationToken);

        var equipmentRepository = serviceProvider.GetRequiredService<IEquipmentRepository>();
        var anyEquipments = await equipmentRepository.AnyAsync(cancellationToken);
        if (!anyEquipments)
            await InitializeEquipmentsAsync(equipmentRepository, seedDataOptions, cancellationToken);

        var exerciseRepository = serviceProvider.GetRequiredService<IExerciseRepository>();
        var anyExercises = await exerciseRepository.AnyAsync(cancellationToken);
        if (!anyExercises)
        {
            var exerciseAliasRepository = serviceProvider.GetRequiredService<IExerciseAliasRepository>();
            await InitializeExercisesAsync(exerciseRepository, muscleRepository, equipmentRepository, exerciseAliasRepository, cancellationToken);
        }

        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
        var anyUsers = await userRepository.AnyUsersAsync();
        if (!anyUsers)
        {
            var workoutRepository = serviceProvider.GetRequiredService<IWorkoutRepository>();
            var workoutRecordRepository = serviceProvider.GetRequiredService<IWorkoutRecordRepository>();
            var exerciseRecordGroupRepository = serviceProvider.GetRequiredService<IExerciseRecordGroupRepository>();
            var exerciseRecordRepository = serviceProvider.GetRequiredService<IExerciseRecordRepository>();
            var exerciseSetGroupRepository = serviceProvider.GetRequiredService<IExerciseSetGroupRepository>();
            var exerciseSetRepository = serviceProvider.GetRequiredService<IExerciseSetRepository>();

            await InitializeUsersAsync(userRepository, workoutRepository, workoutRecordRepository, exerciseRecordGroupRepository, exerciseRecordRepository, exerciseSetGroupRepository, exerciseSetRepository, exerciseRepository, config);
        }
    }

    static async Task InitializeRolesAsync(IRoleRepository roleRepository)
    {
        await RolesInitializer.InitializeAsync(roleRepository, Roles.AdminRole, Roles.UserRole);
    }

    static async Task InitializeUsersAsync(
        IUserRepository userRepository,
        IWorkoutRepository workoutRepository,
        IWorkoutRecordRepository workoutRecordRepository,
        IExerciseRecordGroupRepository exerciseRecordGroupRepository,
        IExerciseRecordRepository exerciseRecordRepository,
        IExerciseSetGroupRepository exerciseSetGroupRepository,
        IExerciseSetRepository exerciseSetRepository,
        IExerciseRepository exerciseRepository,
        IConfiguration config)
    {
        string adminName = config.GetValue<string>("Admin:Name")!;
        string adminPassword = config.GetValue<string>("Admin:Password")!;
        string adminEmail = config.GetValue<string>("Admin:Email")!;

        var user = await UsersInitializer.InitializeAsync(userRepository, adminName, adminEmail, adminPassword, [Roles.AdminRole, Roles.UserRole]);
    }

    static async Task InitializeMusclesAsync(
        IMuscleRepository muscleRepository, 
        SeedDataOptions seedDataOptions, 
        CancellationToken cancellationToken)
    {
        string muscleNamesFilePath = Path.Combine(seedDataOptions.FolderPath, "muscle-names.json");
        string json = await File.ReadAllTextAsync(muscleNamesFilePath, cancellationToken);

        var jsonObject = JObject.Parse(json);
        var musclesArray = (JArray)jsonObject["Muscles"]!;
        var muscleData = musclesArray.ToObject<List<MuscleData>>()!;

        foreach (var muscle in muscleData)
            await MusclesInitializer.InitializeAsync(muscleRepository, muscle, null, cancellationToken);
    }

    static async Task InitializeEquipmentsAsync(
        IEquipmentRepository equipmentRepository, 
        SeedDataOptions seedDataOptions, 
        CancellationToken cancellationToken)
    {
        string equipmentNamesFilePath = Path.Combine(seedDataOptions.FolderPath, "equipment-names.json");
        string json = await File.ReadAllTextAsync(equipmentNamesFilePath, cancellationToken);

        var jsonObject = JObject.Parse(json);
        var equipmentArray = (JArray)jsonObject["Equipments"]!;
        var equipmentNames = equipmentArray.ToObject<List<string>>()!;

        foreach (var equipmentName in equipmentNames)
            await EquipmentInitializer.InitializeAsync(equipmentRepository, equipmentName, cancellationToken);
    }

    static async Task InitializeExercisesAsync(
        IExerciseRepository exerciseRepository,
        IMuscleRepository muscleRepository,
        IEquipmentRepository equipmentRepository,
        IExerciseAliasRepository exerciseAliasRepository,
        CancellationToken cancellationToken)
    {
        async Task InitializeAsync(
            string name,
            string? image,
            string[] equipmentNames,
            ExerciseType exerciseType,
            string[] muscleNames,
            string[]? aliasesStr = null
        )
        {
            await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, exerciseAliasRepository, name, image, equipmentNames, exerciseType, muscleNames, aliasesStr, cancellationToken);
        }

        await InitializeAsync("Pull-ups", GetExercisePhoto("wide_grip_pull_ups.png"), ["Pull-Up Bar"], ExerciseType.Reps, ["Latissimus dorsi", "Biceps brachii short head", "Biceps brachii long head"], ["Pull up", "Pull ups", "Pull-up"]);

        await InitializeAsync("Weighted Cossack Squats", GetExercisePhoto("weighted_cossack_squats.jpg"), ["Dumbbells", "Kettlebells", "Barbells"], ExerciseType.WeightAndReps, ["Quadriceps", "Gluteus maximus", "Hamstrings", "Adductors", "Calves", "Core"]);

        await InitializeAsync("Dumbbell Lateral Raises", GetExercisePhoto("dumbbell_lateral_raises.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Deltoids"], ["Lateral Raises"]);

        await InitializeAsync("Push-ups", GetExercisePhoto("push_ups.jpg"), ["No Equipment"], ExerciseType.Reps, ["Pectoralis major", "Triceps"], ["Push ups", "Push up", "Push-up"]);

        await InitializeAsync("Bench Press", GetExercisePhoto("bench_press.jpg"), ["Barbells"], ExerciseType.WeightAndReps, ["Pectoralis major", "Triceps", "Deltoid anterior"], ["Barbell Bench Press"]);

        await InitializeAsync("Barbell deadlift", GetExercisePhoto("barbell_deadlift.jpg"), ["Barbells"], ExerciseType.WeightAndReps, ["Gluteus maximus", "Hamstrings", "Erector spinae"], ["Deadlift"]);

        await InitializeAsync("Crunches", GetExercisePhoto("do_abdominal_crunches.png"), ["No Equipment"], ExerciseType.Reps, ["Rectus abdominis"]);

        await InitializeAsync("Weighted Crunches", GetExercisePhoto("do_abdominal_crunches.png"), ["Weight Plates"], ExerciseType.WeightAndReps, ["Rectus abdominis"]);

        await InitializeAsync("Shrugs", GetExercisePhoto("shurgs.jpg"), ["Barbells"], ExerciseType.WeightAndReps, ["Trapezius"]);

        await InitializeAsync("Biceps Curl", GetExercisePhoto("biceps_curl.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Biceps brachii short head", "Biceps brachii long head"]);

        await InitializeAsync("Standing Calf Raises", GetExercisePhoto("standing_calf_raises.jpg"), ["Barbells"], ExerciseType.WeightAndReps, ["Gastrocnemius", "Soleus"]);

        await InitializeAsync("Dumbbell Chest Fly", GetExercisePhoto("dumbbell_chest_fly.jpg"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Pectoralis major"], ["Chest Fly"]);

        await InitializeAsync("Seated Overhead Extensions", GetExercisePhoto("seated_dumbbell_overhead_extensions.jpg"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Triceps"]);

        await InitializeAsync("Leg Extension", GetExercisePhoto("leg_extension.png"), ["Leg Extension Machine"], ExerciseType.WeightAndReps, ["Quadriceps"]);

        await InitializeAsync("Single Arm Dumbbell Rows", GetExercisePhoto("single_arm_dumbbell_rows.jpg"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Latissimus dorsi", "Rhomboid major"]);

        await InitializeAsync("Dumbbell Squeeze Press", GetExercisePhoto("dumbbell_squeeze_bench_press.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Pectoralis major", "Triceps"]);

        await InitializeAsync("Seated Dumbbell Wrist Curl", GetExercisePhoto("seated_dumbbell_wrist_curl.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Forearms"], ["Seated Wrist Curl"]);

        await InitializeAsync("Narrow Grip Preacher Curl", GetExercisePhoto("barbell_preacher_curl.png"), ["Preacher Curl Bench"], ExerciseType.WeightAndReps, ["Biceps brachii short head", "Biceps brachii long head"]);

        await InitializeAsync("Bent-over Row", GetExercisePhoto("bent_over_row.jpg"), ["Barbells"], ExerciseType.WeightAndReps, ["Latissimus dorsi", "Rhomboid major"], ["Row", "Bent Over Row"]);

        await InitializeAsync("Barbell Back Squat", GetExercisePhoto("barbell_back_squat.jpg"), ["Barbells"], ExerciseType.WeightAndReps, ["Quadriceps", "Gluteus maximus", "Hamstrings"], ["Squat", "Back Squat"]);

        await InitializeAsync("Weighted Plank", GetExercisePhoto("weighted_plank.jpg"), ["Weighted Vest"], ExerciseType.WeightAndTime, ["Rectus abdominis", "External oblique"]);

        await InitializeAsync("Plank", GetExercisePhoto("plank.png"), ["Weighted Vest"], ExerciseType.Time, ["Rectus abdominis", "External oblique"]);

        await InitializeAsync("Weighted Russian Twists", GetExercisePhoto("weighted_russian_twists.png"), ["Weighted Vest"], ExerciseType.WeightAndReps, ["External oblique"]);

        await InitializeAsync("Russian Twists", GetExercisePhoto("weighted_russian_twists.png"), ["No Equipment"], ExerciseType.Reps, ["External oblique"]);

        await InitializeAsync("Weighted V-ups", GetExercisePhoto("weighted_v_ups.png"), ["Weighted Vest"], ExerciseType.WeightAndReps, ["Rectus abdominis", "Hip flexors"], ["weighted V ups"]);

        await InitializeAsync("Ab Rollouts", GetExercisePhoto("ab_rollouts.png"), ["Core Wheels"], ExerciseType.Reps, ["Rectus abdominis", "Hip flexors"], ["Ab Rollout"]);

        await InitializeAsync("Barbell Sumo Squat", GetExercisePhoto("sumo_squat.png"), ["Barbells"], ExerciseType.WeightAndReps, ["Quadriceps", "Gluteus maximus", "Adductors"], ["Sumo Squat"]);

        await InitializeAsync("Weighted Russian Twists (Legs Up)", GetExercisePhoto("weighted_russian_twists_legs_up.png"), ["Weighted Vest"], ExerciseType.Reps, ["External oblique", "Rectus abdominis"]);

        await InitializeAsync("Clap Push-ups", GetExercisePhoto("clap_push_ups.png"), ["No Equipment"], ExerciseType.Reps, ["Pectoralis major", "Triceps", "Deltoids"], ["Clap Push up"]);

        await InitializeAsync("Renegade Row Push-ups", GetExercisePhoto("dumbbell_renegade_row_push_ups.png"), ["No Equipment"], ExerciseType.Reps, ["Latissimus dorsi", "Triceps", "Core"], ["Renegade Row Push ups"]);

        await InitializeAsync("Weighted Push-ups", GetExercisePhoto("weighted_push_ups.png"), ["Weighted Vest"], ExerciseType.WeightAndReps, ["Pectoralis major", "Triceps"], ["Weighted Push ups"]);

        await InitializeAsync("Elevated Push-ups", GetExercisePhoto("elevated_push_ups.png"), ["No Equipment"], ExerciseType.Reps, ["Pectoralis major", "Triceps"], ["Elevated Push ups"]);

        await InitializeAsync("Weighted Pull-ups", GetExercisePhoto("weighted_pull_ups.png"), ["Pull-Up Bar", "Weighted Vest"], ExerciseType.WeightAndReps, ["Latissimus dorsi", "Biceps brachii short head", "Biceps brachii long head"], ["Weighted Pull up"]);

        await InitializeAsync("Wide Grip Pull-ups", GetExercisePhoto("wide_grip_pull_ups.png"), ["Pull-Up Bar"], ExerciseType.Reps, ["Latissimus dorsi", "Biceps brachii short head", "Biceps brachii long head"], ["Wide Pull up", "Wide Pull-up"]);

        await InitializeAsync("Uneven Push-ups on a Ball", GetExercisePhoto("uneven_push_ups_on_a_ball.png"), ["No Equipment"], ExerciseType.Reps, ["Pectoralis major", "Triceps"], ["Uneven Push ups on a ball"]);

        await InitializeAsync("Weighted Side Plank", GetExercisePhoto("side_plank.png"), ["Weighted Vest"], ExerciseType.WeightAndTime, ["External oblique"]);

        await InitializeAsync("Upright Row", GetExercisePhoto("upright_row.png"), ["Barbells"], ExerciseType.WeightAndReps, ["Deltoids", "Trapezius"]);

        await InitializeAsync("Seated In-and-Out", GetExercisePhoto("seated_in-and-out.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Core"], ["Seated In and Out"]);

        await InitializeAsync("Toes to Bar", GetExercisePhoto("toes_to_bar.jpg"), ["Pull-Up Bar"], ExerciseType.Reps, ["Rectus abdominis", "Hip flexors"]);

        await InitializeAsync("Weighted Hanging Knee Raises", GetExercisePhoto("weighted_hanging_knee_raises.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Rectus abdominis", "Hip flexors"]);

        await InitializeAsync("Hanging Knee Raises", GetExercisePhoto("weighted_hanging_knee_raises.png"), ["No Equipment"], ExerciseType.Reps, ["Rectus abdominis", "Hip flexors"]);

        await InitializeAsync("Front Squat", GetExercisePhoto("front_squat.png"), ["Barbells"], ExerciseType.WeightAndReps, ["Quadriceps", "Gluteus maximus"]);

        await InitializeAsync("Standing Side Bend", GetExercisePhoto("standing_side_bend.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["External oblique"]);

        await InitializeAsync("Crossbody Hammer Curl", GetExercisePhoto("crossbody_hammer_curl.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Biceps brachii short head", "Biceps brachii long head"]);

        await InitializeAsync("Band Bicycle Crunch", GetExercisePhoto("band_bicycle_crunch.png"), ["Resistance Bands"], ExerciseType.Reps, ["Rectus abdominis", "External oblique"]);

        await InitializeAsync("Hollow Body Hold", GetExercisePhoto("hollow_body_hold.jpg"), ["No Equipment"], ExerciseType.Time, ["Rectus abdominis"]);

        await InitializeAsync("Bent-Over Reverse Fly", GetExercisePhoto("bent-over_reverse_fly.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Deltoids", "Rhomboid major"], ["Bent Over Reverse Fly", "Reverse Fly"]);

        await InitializeAsync("Seated Overhead Press", GetExercisePhoto("seated_overhead_press.png"), ["Barbells"], ExerciseType.WeightAndReps, ["Deltoids", "Triceps"]);

        await InitializeAsync("Reverse Dumbbell Wrist Curl", GetExercisePhoto("reverse_dumbbell_wrist_curl.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Forearms"]);

        await InitializeAsync("Jogging", GetExercisePhoto("jogging_2.png"), ["Treadmill"], ExerciseType.Time, ["Core", "Legs"]);

        await InitializeAsync("Lying Weighted Neck Flexion", GetExercisePhoto("lying_weighted_neck_flexion.jpg"), ["Weight Plates"], ExerciseType.WeightAndReps, ["Sternocleidomastoid"]);

        await InitializeAsync("Lying Weighted Neck Extension", GetExercisePhoto("lying_weighted_neck_extension.png"), ["Weight Plates"], ExerciseType.WeightAndReps, ["Trapezius"]);

        await InitializeAsync("Plate Front Raise", GetExercisePhoto("plate_front_raise.png"), ["Weight Plates"], ExerciseType.WeightAndReps, ["Deltoids"]);

        await InitializeAsync("Hanging Leg Raise", GetExercisePhoto("hanging_leg_raises.png"), ["No Equipment"],
            ExerciseType.WeightAndReps, ["Abs"]);

        await InitializeAsync("Sumo Deadlift", GetExercisePhoto("sumo_deadlift.png"), ["Barbells"],
            ExerciseType.WeightAndReps, ["Hamstrings", "Glutes"]);

        await InitializeAsync("Dips", GetExercisePhoto("dips.png"), ["Dip Bars"], ExerciseType.Reps, ["Triceps", "Chest"]);

        await InitializeAsync("Weighted Dips", GetExercisePhoto("dips.png"), ["Dip Bars", "Weight Plates"], ExerciseType.WeightAndReps, ["Triceps", "Chest"]);

        await InitializeAsync("Bench Dips", GetExercisePhoto("bench_dips.png"), ["Bench"],
            ExerciseType.Reps, ["Triceps", "Chest"]);

        await InitializeAsync("Weighted Bench Dips", GetExercisePhoto("bench_dips.png"), ["Bench", "Weight Plates"],
            ExerciseType.WeightAndReps, ["Triceps", "Chest"]);

        await InitializeAsync("Hammers", GetExercisePhoto("hammers.png"), ["Dumbbells"],
            ExerciseType.WeightAndReps, ["Forearms"]);

        await InitializeAsync("Incline Bench Press", GetExercisePhoto("incline_bench_press.png"), ["Barbells", "Bench"],
            ExerciseType.WeightAndReps, ["Chest", "Deltoids"]);

        await InitializeAsync("Triceps Pushdown", GetExercisePhoto("triceps_pushdown.png"), ["Cable Machine"],
            ExerciseType.WeightAndReps, ["Triceps"]);

        await InitializeAsync("Neutral Grip Lat Pulldown", GetExercisePhoto("neutral_grip_lat_pulldown.png"), ["Cable Machine"],
            ExerciseType.WeightAndReps, ["Latissimus Dorsi"]);

        await InitializeAsync("Burpees", GetExercisePhoto("burpee.png"), ["No Equipment"], ExerciseType.Reps, ["Core", "Legs"]);

        await InitializeAsync("Cable Crunches", GetExercisePhoto("cable_crunches.png"), ["Cable Machine"], ExerciseType.WeightAndReps, ["Abs"]);

        await InitializeAsync("Lunges", GetExercisePhoto("lunges.png"), ["Dumbbells"], ExerciseType.WeightAndReps, ["Quadriceps", "Glutes", "Hamstrings"]);
    }

    static string GetExercisePhoto(string fileName)
    {
        string exercisePhotosPath = Path.Combine("images", "exercises");
        return Path.Combine(exercisePhotosPath, fileName);
    }
}