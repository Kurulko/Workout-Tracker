using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using WorkoutTracker.API.Bindings;
using WorkoutTracker.API.Filters;
using WorkoutTracker.Infrastructure.Extensions;
using WorkoutTracker.Application.Extensions;
using WorkoutTracker.Persistence.Extensions;
using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Persistence.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WorkoutTracker.Application.Common.Settings;
using FluentValidation.AspNetCore;
using WorkoutTracker.API.Bindings.Providers;

namespace WorkoutTracker.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPersistence(configuration);
        services.AddInfrastructure(configuration);
        services.AddApplication();

        services.AddFluentValidationAutoValidation();

        services.AddIdentity();
        services.AddIdentityOptions();

        services.AddJWTAuthentication(configuration);
        services.AddSessionKeysOptions(configuration);
        services.AddSeedDataOptions(configuration);
        services.AddDefaultCors();

        return services;
    }

    public static void AddIdentity(this IServiceCollection services)
    {
        services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<WorkoutDbContext>()
            .AddDefaultTokenProviders();
    }

    static void AddIdentityOptions(this IServiceCollection services)
    {
        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
        });
    }

    static void AddJWTAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
        services.AddSingleton(jwtSettings);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
            options.TokenValidationParameters = jwtSettings.ToTokenValidationParameters()
        );
    } 
    
    static void AddSessionKeysOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var sessionKeysOptions = configuration.GetSection("SessionKeys").Get<SessionKeysOptions>()!;
        services.AddSingleton(sessionKeysOptions);
    }

    static void AddSeedDataOptions(this IServiceCollection services, IConfiguration configuration)
    {
        var seedDataOptions = configuration.GetSection("SeedData").Get<SeedDataOptions>()!;
        services.AddSingleton(seedDataOptions);
    }

    static void AddDefaultCors(this IServiceCollection services)
    {
        services.AddCors(options =>
           options.AddDefaultPolicy(builder =>
               builder.WithOrigins("*")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
       ));
    }


    public static void AddControllersWithOptions(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        services.AddControllers(options =>
        {
            options.Filters.Add<ValidateModelStateFilter>();
            options.ModelBinderProviders.Insert(0, new DateTimeRangeBinderProvider());
            options.ModelBinderProviders.Insert(1, new FileUploadModelBinderProvider());
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
}
