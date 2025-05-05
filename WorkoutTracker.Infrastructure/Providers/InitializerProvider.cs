using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
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
    const string sourceFolderPath = "Data/Source";

    public static async Task InitializeDataAsync(this WebApplication app, ConfigurationManager config)
    {
        using IServiceScope serviceScope = app.Services.CreateScope();

        IServiceProvider serviceProvider = serviceScope.ServiceProvider;

        var fileService = serviceProvider.GetRequiredService<IFileService>();

        var roleRepository = serviceProvider.GetRequiredService<IRoleRepository>();
        if (!await roleRepository.AnyAsync())
            await InitializeRolesAsync(roleRepository);

        var muscleRepository = serviceProvider.GetRequiredService<IMuscleRepository>();
        if (!await muscleRepository.AnyAsync())
            await InitializeMusclesAsync(muscleRepository);

        var equipmentRepository = serviceProvider.GetRequiredService<IEquipmentRepository>();
        if (!await equipmentRepository.AnyAsync())
            await InitializeEquipmentsAsync(equipmentRepository);

        var exerciseRepository = serviceProvider.GetRequiredService<IExerciseRepository>();
        if (!await exerciseRepository.AnyAsync())
            await InitializeExercisesAsync(exerciseRepository, muscleRepository, equipmentRepository);

        var userRepository = serviceProvider.GetRequiredService<IUserRepository>();

        if (!await userRepository.AnyUsersAsync())
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
        => await RolesInitializer.InitializeAsync(roleRepository, Roles.AdminRole, Roles.UserRole);

    static async Task InitializeUsersAsync(
        IUserRepository userRepository, 
        IWorkoutRepository workoutRepository, 
        IWorkoutRecordRepository workoutRecordRepository, 
        IExerciseRecordGroupRepository exerciseRecordGroupRepository, 
        IExerciseRecordRepository exerciseRecordRepository, 
        IExerciseSetGroupRepository exerciseSetGroupRepository, 
        IExerciseSetRepository exerciseSetRepository,
        IExerciseRepository exerciseRepository, 
        ConfigurationManager config)
    {
        string adminName = config.GetValue<string>("Admin:Name")!;
        string adminPassword = config.GetValue<string>("Admin:Password")!;
        string adminEmail = config.GetValue<string>("Admin:Email")!;

        var user = await UsersInitializer.InitializeAsync(userRepository, adminName, adminEmail, adminPassword, [ Roles.AdminRole, Roles.UserRole ]);

        await WorkoutInitializerProvider.InitializeWorkoutsAsync(workoutRepository, workoutRecordRepository, exerciseRecordGroupRepository, exerciseRecordRepository, exerciseSetGroupRepository, exerciseSetRepository, exerciseRepository, user.Id);
    }

    static async Task InitializeMusclesAsync(IMuscleRepository muscleRepository)
    {
        string muscleNamesFilePath = Path.Combine(sourceFolderPath, "muscle-names.json");
        string json = await File.ReadAllTextAsync(muscleNamesFilePath);

        var jsonObject = JObject.Parse(json);
        var musclesArray = (JArray)jsonObject["Muscles"]!;
        var muscleData = musclesArray.ToObject<List<MuscleData>>()!;

        foreach (var muscle in muscleData)
            await MusclesInitializer.InitializeAsync(muscleRepository, muscle, null);
    }

    static async Task InitializeEquipmentsAsync(IEquipmentRepository equipmentRepository)
    {
        string equipmentNamesFilePath = Path.Combine(sourceFolderPath, "equipment-names.json");
        string json = await File.ReadAllTextAsync(equipmentNamesFilePath);

        var jsonObject = JObject.Parse(json);
        var equipmentArray = (JArray)jsonObject["Equipments"]!;
        var equipmentNames = equipmentArray.ToObject<List<string>>()!;

        foreach (var equipmentName in equipmentNames)
            await EquipmentInitializer.InitializeAsync(equipmentRepository, equipmentName);
    }

    static async Task InitializeExercisesAsync(
        IExerciseRepository exerciseRepository,
        IMuscleRepository muscleRepository,
        IEquipmentRepository equipmentRepository)
    {
        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Pull-ups", GetExercisePhoto("wide_grip_pull_ups.png"), [ "Pull-Up Bar" ], ExerciseType.Reps, [ "Latissimus dorsi", "Biceps brachii short head", "Biceps brachii long head" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Cossack Squats", GetExercisePhoto("weighted_cossack_squats.jpg"), [ "Dumbbells", "Kettlebells", "Barbells" ],
        ExerciseType.WeightAndReps, [ "Quadriceps", "Gluteus maximus", "Hamstrings", "Adductors", "Calves", "Core" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Dumbbell Lateral Raises", GetExercisePhoto("dumbbell_lateral_raises.png"), [ "Dumbbells"], ExerciseType.WeightAndReps, [ "Deltoids"]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Push-ups", GetExercisePhoto("push_ups.jpg"), [ "No Equipment" ], ExerciseType.Reps, [ "Pectoralis major", "Triceps" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Bench Press", GetExercisePhoto("bench_press.jpg"), [ "Barbells"], ExerciseType.WeightAndReps, [ "Pectoralis major", "Triceps", "Deltoid anterior" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Barbell deadlift", GetExercisePhoto("barbell_deadlift.jpg"), [ "Barbells"], ExerciseType.WeightAndReps, [ "Gluteus maximus", "Hamstrings", "Erector spinae" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Crunches", GetExercisePhoto("do_abdominal_crunches.png"), [ "No Equipment" ], ExerciseType.Reps, [ "Rectus abdominis" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Crunches", GetExercisePhoto("do_abdominal_crunches.png"), [ "Weight Plates"], ExerciseType.WeightAndReps, [ "Rectus abdominis" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Shrugs", GetExercisePhoto("shurgs.jpg"), [ "Barbells"], ExerciseType.WeightAndReps, [ "Trapezius" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Biceps Curl", GetExercisePhoto("biceps_curl.png"), [ "Dumbbells"], ExerciseType.WeightAndReps, [ "Biceps brachii short head", "Biceps brachii long head"]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Standing Calf Raises", GetExercisePhoto("standing_calf_raises.jpg"), [ "Barbells" ], ExerciseType.WeightAndReps, [ "Gastrocnemius", "Soleus" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Dumbbell Chest Fly", GetExercisePhoto("dumbbell_chest_fly.jpg"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Pectoralis major" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Seated Overhead Extensions", GetExercisePhoto("seated_dumbbell_overhead_extensions.jpg"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Triceps"]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Leg Extension", GetExercisePhoto("leg_extension.png"), [ "Leg Extension Machine" ], ExerciseType.WeightAndReps, [ "Quadriceps" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Single Arm Dumbbell Rows", GetExercisePhoto("single_arm_dumbbell_rows.jpg"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Latissimus dorsi", "Rhomboid major" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Dumbbell Squeeze Press", GetExercisePhoto("dumbbell_squeeze_bench_press.png"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Pectoralis major", "Triceps" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Seated Dumbbell Wrist Curl", GetExercisePhoto("seated_dumbbell_wrist_curl.png"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Forearms" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Narrow Grip Preacher Curl", GetExercisePhoto("barbell_preacher_curl.png"), [ "Preacher Curl Bench" ], ExerciseType.WeightAndReps, [ "Biceps brachii short head", "Biceps brachii long head"]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Bent-over Row", GetExercisePhoto("bent_over_row.jpg"), [ "Barbells"], ExerciseType.WeightAndReps, [ "Latissimus dorsi", "Rhomboid major" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Barbell Back Squat", GetExercisePhoto("barbell_back_squat.jpg"), [ "Barbells" ], ExerciseType.WeightAndReps, [ "Quadriceps", "Gluteus maximus", "Hamstrings" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Plank", GetExercisePhoto("weighted_plank.jpg"), [ "Weighted Vest" ], ExerciseType.WeightAndTime, [ "Rectus abdominis", "External oblique" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Plank", GetExercisePhoto("plank.png"), [ "Weighted Vest" ], ExerciseType.Time, [ "Rectus abdominis", "External oblique" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Russian Twists", GetExercisePhoto("weighted_russian_twists.png"), [ "Weighted Vest" ], ExerciseType.WeightAndReps, [ "External oblique" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Russian Twists", GetExercisePhoto("weighted_russian_twists.png"), [ "No Equipment" ], ExerciseType.Reps, [ "External oblique" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted V-ups", GetExercisePhoto("weighted_v_ups.png"), [ "Weighted Vest" ], ExerciseType.WeightAndReps, [ "Rectus abdominis", "Hip flexors"]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Ab Rollouts", GetExercisePhoto("ab_rollouts.png"), [ "Core Wheels" ], ExerciseType.Reps, [ "Rectus abdominis", "Hip flexors" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Barbell Sumo Squat", GetExercisePhoto("sumo_squat.png"), [ "Barbells" ], ExerciseType.WeightAndReps, [ "Quadriceps", "Gluteus maximus", "Adductors"]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Russian Twists (Legs Up)", GetExercisePhoto("weighted_russian_twists_legs_up.png"), [ "Weighted Vest" ], ExerciseType.Reps, [ "External oblique", "Rectus abdominis"]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Clap Push-ups", GetExercisePhoto("clap_push_ups.png"), [ "No Equipment" ], ExerciseType.Reps, [ "Pectoralis major", "Triceps", "Deltoids"]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Renegade Row Push-ups", GetExercisePhoto("dumbbell_renegade_row_push_ups.png"), [ "No Equipment" ], ExerciseType.Reps, [ "Latissimus dorsi", "Triceps", "Core" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Push-ups", GetExercisePhoto("weighted_push_ups.png"), [ "Weighted Vest" ], ExerciseType.WeightAndReps, [ "Pectoralis major", "Triceps" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Elevated Push-ups", GetExercisePhoto("elevated_push_ups.png"), [ "No Equipment" ], ExerciseType.Reps, [ "Pectoralis major", "Triceps" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Pull-ups", GetExercisePhoto("weighted_pull_ups.png"), [ "Pull-Up Bar", "Weighted Vest" ], ExerciseType.WeightAndReps, [ "Latissimus dorsi", "Biceps brachii short head", "Biceps brachii long head" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Wide Grip Pull-ups", GetExercisePhoto("wide_grip_pull_ups.png"), [ "Pull-Up Bar" ], ExerciseType.Reps, [ "Latissimus dorsi", "Biceps brachii short head", "Biceps brachii long head" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Uneven Push-ups on a Ball", GetExercisePhoto("uneven_push_ups_on_a_ball.png"), [ "No Equipment" ], ExerciseType.Reps, [ "Pectoralis major", "Triceps" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Side Plank", GetExercisePhoto("side_plank.png"), [ "Weighted Vest" ], ExerciseType.WeightAndTime, [ "External oblique" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Upright Row", GetExercisePhoto("upright_row.png"), [ "Barbells" ], ExerciseType.WeightAndReps, [ "Deltoids", "Trapezius" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Seated In-and-Out", GetExercisePhoto("seated_in-and-out.png"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Core" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Toes to Bar", GetExercisePhoto("toes_to_bar.jpg"), [ "Pull-Up Bar" ], ExerciseType.Reps, [ "Rectus abdominis", "Hip flexors" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Weighted Hanging Knee Raises", GetExercisePhoto("weighted_hanging_knee_raises.png"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Rectus abdominis", "Hip flexors" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Hanging Knee Raises", GetExercisePhoto("weighted_hanging_knee_raises.png"), [ "No Equipment" ], ExerciseType.Reps, [ "Rectus abdominis", "Hip flexors" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Front Squat", GetExercisePhoto("front_squat.png"), [ "Barbells" ], ExerciseType.WeightAndReps, [ "Quadriceps", "Gluteus maximus" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Standing Side Bend", GetExercisePhoto("standing_side_bend.png"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "External oblique" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Crossbody Hammer Curl", GetExercisePhoto("crossbody_hammer_curl.png"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Biceps brachii short head", "Biceps brachii long head" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Band Bicycle Crunch", GetExercisePhoto("band_bicycle_crunch.png"), [ "Resistance Bands" ], ExerciseType.Reps, [ "Rectus abdominis", "External oblique" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Hollow Body Hold", GetExercisePhoto("hollow_body_hold.jpg"), [ "No Equipment" ], ExerciseType.Time, [ "Rectus abdominis" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Bent-Over Reverse Fly", GetExercisePhoto("bent-over_reverse_fly.png"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Deltoids", "Rhomboid major" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Seated Overhead Press", GetExercisePhoto("seated_overhead_press.png"), [ "Barbells" ], ExerciseType.WeightAndReps, [ "Deltoids", "Triceps" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Reverse Dumbbell Wrist Curl", GetExercisePhoto("reverse_dumbbell_wrist_curl.png"), [ "Dumbbells" ], ExerciseType.WeightAndReps, [ "Forearms" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Jogging", GetExercisePhoto("jogging_2.png"), [ "Treadmill" ], ExerciseType.Time, [ "Core", "Legs" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Lying Weighted Neck Flexion", GetExercisePhoto("lying_weighted_neck_flexion.jpg"), [ "Weight Plates" ], ExerciseType.WeightAndReps, [ "Sternocleidomastoid" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Lying Weighted Neck Extension", GetExercisePhoto("lying_weighted_neck_extension.png"), [ "Weight Plates" ], ExerciseType.WeightAndReps, [ "Trapezius" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository, "Plate Front Raise", GetExercisePhoto("plate_front_raise.png"), [ "Weight Plates" ], ExerciseType.WeightAndReps, [ "Deltoids" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Hanging Leg Raise", GetExercisePhoto("hanging_leg_raises.png"), [ "No Equipment" ],
            ExerciseType.WeightAndReps, [ "Abs" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Sumo Deadlift", GetExercisePhoto("sumo_deadlift.png"), [ "Barbells" ],
            ExerciseType.WeightAndReps, [ "Hamstrings", "Glutes" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Dips", GetExercisePhoto("dips.png"), [ "Dip Bars" ], ExerciseType.Reps, [ "Triceps", "Chest" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Weighted Dips", GetExercisePhoto("dips.png"), [ "Dip Bars", "Weight Plates" ], ExerciseType.WeightAndReps, [ "Triceps", "Chest" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Bench Dips", GetExercisePhoto("bench_dips.png"), [ "Bench" ],
            ExerciseType.Reps, [ "Triceps", "Chest" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Weighted Bench Dips", GetExercisePhoto("bench_dips.png"), [ "Bench", "Weight Plates" ],
            ExerciseType.WeightAndReps, [ "Triceps", "Chest" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Hammers", GetExercisePhoto("hammers.png"), [ "Dumbbells" ],
            ExerciseType.WeightAndReps, [ "Forearms" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Incline Bench Press", GetExercisePhoto("incline_bench_press.png"), [ "Barbells", "Bench" ],
            ExerciseType.WeightAndReps, [ "Chest", "Deltoids" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Triceps Pushdown", GetExercisePhoto("triceps_pushdown.png"), [ "Cable Machine" ],
            ExerciseType.WeightAndReps, [ "Triceps" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Neutral Grip Lat Pulldown", GetExercisePhoto("neutral_grip_lat_pulldown.png"), [ "Cable Machine" ],
            ExerciseType.WeightAndReps, [ "Latissimus Dorsi" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Burpees", GetExercisePhoto("burpee.png"), [ "No Equipment" ],
            ExerciseType.Reps, [ "Core", "Legs" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Cable Crunches", GetExercisePhoto("cable_crunches.png"), [ "Cable Machine" ],
            ExerciseType.WeightAndReps, [ "Abs" ]);

        await ExercisesInitializer.InitializeAsync(exerciseRepository, muscleRepository, equipmentRepository,
            "Lunges", GetExercisePhoto("lunges.png"), [ "Dumbbells" ],
            ExerciseType.WeightAndReps, [ "Quadriceps", "Glutes", "Hamstrings" ]);

    }

    static string GetExercisePhoto(string fileName)
    {
        string exercisePhotosPath = Path.Combine("photos", "exercises");
        return Path.Combine(exercisePhotosPath, fileName);
    }
}