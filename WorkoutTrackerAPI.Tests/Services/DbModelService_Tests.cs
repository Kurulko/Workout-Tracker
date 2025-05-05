using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories.UserRepositories;

namespace WorkoutTracker.API.Tests.Services;

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