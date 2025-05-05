using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Data.Models;
using WorkoutTracker.API.Data;
using WorkoutTracker.API.Initializers;
using WorkoutTracker.API.Repositories.UserRepositories;
using WorkoutTracker.API.Controllers;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace WorkoutTracker.API.Tests.Controllers;

public class DbModelController_Tests<T> : APIController_Tests
    where T : class, IDbModel
{
    protected readonly Mock<IHttpContextAccessor> mockHttpContextAccessor = new();

    protected void SetupMockHttpContextAccessor(string userId)
    {
        var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
           new (ClaimTypes.NameIdentifier, userId)
        }, "Mock"));

        var mockHttpContext = new DefaultHttpContext
        {
            User = claimsPrincipal
        };

        mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext);
    }

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
