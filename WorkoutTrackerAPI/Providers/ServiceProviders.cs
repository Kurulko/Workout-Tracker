using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Settings;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.AccountServices;
using WorkoutTrackerAPI.Services.BodyWeightServices;
using WorkoutTrackerAPI.Services.EquipmentServices;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Services.ImpersonationServices;
using WorkoutTrackerAPI.Services.MuscleServices;
using WorkoutTrackerAPI.Services.MuscleSizeServices;
using WorkoutTrackerAPI.Services.ProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.BaseInfoProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.BodyWeightProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.StrikeDurationProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.TotalCompletedProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.WorkoutDurationProgressServices;
using WorkoutTrackerAPI.Services.ProgressServices.WorkoutWeightLiftedProgressServices;
using WorkoutTrackerAPI.Services.RoleServices;
using WorkoutTrackerAPI.Services.WorkoutProgressServices;
using WorkoutTrackerAPI.Services.UserServices;
using WorkoutTrackerAPI.Services.WorkoutRecordServices;
using WorkoutTrackerAPI.Services.WorkoutServices;
using WorkoutTrackerAPI.ValidationAttributes;
using WorkoutTrackerAPI.Services.ExerciseAliasServices;

namespace WorkoutTrackerAPI.Providers;

public static class ServiceProviders
{
    public static void AddControllersWithOptions(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddControllers(options =>
        {
            options.Filters.Add<ValidateModelStateAttribute>();
        })
        .AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        })
        .AddJsonOptions(options =>
        {
            //options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            //options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    }

    public static void AddMSSQLServer(this IServiceCollection services, IConfiguration configuration)
    {
        string connection = configuration.GetConnectionString("DefaultConnection")!;
        services.AddDbContext<WorkoutDbContext>(opts =>
        {
            opts.UseSqlServer(connection);
            opts.EnableSensitiveDataLogging();
        });
    }

    public static void AddIdentityModels(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<WorkoutDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8; 
            options.Password.RequireLowercase = true; 
            options.Password.RequireUppercase = true; 
            options.Password.RequireNonAlphanumeric = true; 
        });
    }


    public static void AddJWTAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
            options.TokenValidationParameters = (TokenValidationParameters)jwtSettings
        );

        services.AddCors(options =>
            options.AddDefaultPolicy(builder =>
                builder.WithOrigins("*")
                       .AllowAnyHeader()
                       .AllowAnyMethod()
        ));
    }

    public static void AddAccountServices(this IServiceCollection services)
    {
        services.AddAccountRepositories();
        services.AddScoped<JwtHandler>();
        services.AddScoped<IImpersonationService, ImpersonationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAccountService, AccountService>();
    }

    static void AddAccountRepositories(this IServiceCollection services)
    {
        services.AddScoped<UserRepository>();
        services.AddScoped<RoleRepository>();
    }


    public static void AddWorkoutModelServices(this IServiceCollection services)
    {
        services.AddWorkoutModelRepositories();
        services.AddScoped<IBodyWeightService, BodyWeightService>();
        services.AddScoped<IExerciseService, ExerciseService>();
        services.AddScoped<IExerciseAliasService, ExerciseAliasService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IMuscleService, MuscleService>();
        services.AddScoped<IMuscleSizeService, MuscleSizeService>();
        services.AddScoped<IExerciseRecordService, ExerciseRecordService>();
        services.AddScoped<IWorkoutService, WorkoutService>();
        services.AddScoped<IWorkoutRecordService, WorkoutRecordService>();
        services.AddProgressServices();
    }

    static void AddWorkoutModelRepositories(this IServiceCollection services)
    {
        services.AddScoped<UserDetailsRepository>();
        services.AddScoped<BodyWeightRepository>();
        services.AddScoped<ExerciseRepository>();
        services.AddScoped<ExerciseAliasRepository>();
        services.AddScoped<EquipmentRepository>();
        services.AddScoped<MuscleRepository>();
        services.AddScoped<MuscleSizeRepository>();
        services.AddScoped<ExerciseSetRepository>();
        services.AddScoped<ExerciseSetGroupRepository>();
        services.AddScoped<ExerciseRecordRepository>();
        services.AddScoped<ExerciseRecordGroupRepository>();
        services.AddScoped<WorkoutRecordRepository>();
        services.AddScoped<WorkoutRepository>();
    }

    static void AddProgressServices(this IServiceCollection services)
    {
        services.AddSingleton<IBaseInfoProgressService, BaseInfoProgressService>();

        services.AddSingleton<ITotalCompletedProgressService, TotalCompletedProgressService>();
        services.AddSingleton<IWorkoutWeightLiftedProgressService, WorkoutWeightLiftedProgressService>();
        services.AddSingleton<IStrikeDurationProgressService, StrikeDurationProgressService>();
        services.AddSingleton<IWorkoutDurationProgressService, WorkoutDurationProgressService>();
        services.AddSingleton<IBodyWeightProgressService, BodyWeightProgressService>();

        services.AddScoped<IWorkoutProgressService, WorkoutProgressService>();
    }
}