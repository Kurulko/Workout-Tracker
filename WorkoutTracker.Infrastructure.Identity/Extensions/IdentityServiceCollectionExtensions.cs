using Microsoft.Extensions.DependencyInjection;
using WorkoutTracker.Infrastructure.Identity.Profiles;

namespace WorkoutTracker.Infrastructure.Identity.Extensions;

public static class IdentityServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityInfrastructure(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(UserProfile).Assembly);

        return services;
    }
}