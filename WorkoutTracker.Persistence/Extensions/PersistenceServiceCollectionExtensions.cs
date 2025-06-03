using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Application.Interfaces.Repositories.Workouts;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Persistence.Repositories;
using WorkoutTracker.Persistence.Repositories.Exercises;
using WorkoutTracker.Persistence.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Persistence.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Persistence.Repositories.Muscles;
using WorkoutTracker.Persistence.Repositories.Users;
using WorkoutTracker.Persistence.Repositories.Workouts;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Extensions;

public static class PersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMSSQLServer(configuration);

        services.AddAccountRepositories();
        services.AddWorkoutModelRepositories();

        return services;
    }

    static IServiceCollection AddMSSQLServer(this IServiceCollection services, IConfiguration configuration)
    {
        string connection = configuration.GetConnectionString("DefaultConnection")!;
        services.AddDbContext<WorkoutDbContext>(opts =>
        {
            opts.UseSqlServer(connection);
            opts.EnableSensitiveDataLogging();
        });

        return services;
    }

    static IServiceCollection AddAccountRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();

        return services;
    }

    static IServiceCollection AddWorkoutModelRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserDetailsRepository, UserDetailsRepository>();
        services.AddScoped<IBodyWeightRepository, BodyWeightRepository>();
        services.AddScoped<IEquipmentRepository, EquipmentRepository>();

        services.AddExerciseRepositories();
        services.AddMuscleRepositories();
        services.AddWorkoutRepositories();

        return services;
    }

    static IServiceCollection AddExerciseRepositories(this IServiceCollection services)
    {
        services.AddScoped<IExerciseRepository, ExerciseRepository>();
        services.AddScoped<IExerciseAliasRepository, ExerciseAliasRepository>();
        services.AddScoped<IExerciseSetRepository, ExerciseSetRepository>();
        services.AddScoped<IExerciseSetGroupRepository, ExerciseSetGroupRepository>();
        services.AddScoped<IExerciseRecordRepository, ExerciseRecordRepository>();
        services.AddScoped<IExerciseRecordGroupRepository, ExerciseRecordGroupRepository>();

        return services;
    }

    static IServiceCollection AddMuscleRepositories(this IServiceCollection services)
    {
        services.AddScoped<IMuscleRepository, MuscleRepository>();
        services.AddScoped<IMuscleAliasRepository, MuscleAliasRepository>();
        services.AddScoped<IMuscleSizeRepository, MuscleSizeRepository>();

        return services;
    }

    static IServiceCollection AddWorkoutRepositories(this IServiceCollection services)
    {
        services.AddScoped<IWorkoutRecordRepository, WorkoutRecordRepository>();
        services.AddScoped<IWorkoutRepository, WorkoutRepository>();

        return services;
    }
}