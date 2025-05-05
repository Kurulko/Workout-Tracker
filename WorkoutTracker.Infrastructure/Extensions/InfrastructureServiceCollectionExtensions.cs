using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkoutTracker.Application.Common.Settings;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Services.Auth;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Application.Interfaces.Services.Progresses;
using WorkoutTracker.Application.Interfaces.Services.Workouts;
using WorkoutTracker.Infrastructure.Auth;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Services;
using WorkoutTracker.Infrastructure.Services;
using WorkoutTracker.Infrastructure.Services.Auth;
using WorkoutTracker.Infrastructure.Services.Exercises;
using WorkoutTracker.Infrastructure.Services.Muscles;
using WorkoutTracker.Infrastructure.Services.Progresses;
using WorkoutTracker.Infrastructure.Services.Workouts;
using WorkoutTracker.Infrastructure.Identity.Extensions;

namespace WorkoutTracker.Infrastructure.Extensions;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IFileService, FileService>();

        services.AddAccountServices();
        services.AddWorkoutModelServices();

        services.AddIdentityInfrastructure();

        return services;
    }

    static void AddAccountServices(this IServiceCollection services)
    {
        services.AddScoped<JwtHandler>();
        services.AddScoped<IImpersonationService, ImpersonationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IAccountService, AccountService>();
    }

    static void AddWorkoutModelServices(this IServiceCollection services)
    {
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
