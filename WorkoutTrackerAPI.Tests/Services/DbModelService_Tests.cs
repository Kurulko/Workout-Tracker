using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Initializers;
using WorkoutTrackerAPI.Repositories.UserRepositories;

namespace WorkoutTrackerAPI.Tests.Services;

public abstract class DbModelService_Tests<TModel> : BaseService_Tests where TModel : class, IDbModel
{
    protected async Task<User> GetDefaultUserAsync(WorkoutDbContext db)
    {
        var userManager = IdentityHelper.GetUserManager(db);
        var userRepository = new UserRepository(userManager, db);

        string name = "User";
        string email = "user@email.com";
        string password = "P@$$w0rd";

        var user = await userRepository.GetUserByUsernameAsync(name);

        if (user is null)
        {
            await WorkoutContextFactory.InitializeRolesAsync(db);
            user = await UsersInitializer.InitializeAsync(userRepository, name, email, password, new[] { Roles.UserRole });
        }

        return user;
    }
}