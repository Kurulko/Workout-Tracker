using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Tests;

public static class IdentityHelper
{
    public static RoleManager<Role> GetRoleManager(WorkoutDbContext db)
    {
        var roleStore = new RoleStore<Role>(db);
        //return new RoleManager<Role>(
        //    roleStore,
        //    new IRoleValidator<Role>[0],
        //    new UpperInvariantLookupNormalizer(),
        //    new Mock<IdentityErrorDescriber>().Object,
        //    new Mock<ILogger<RoleManager<Role>>>().Object);
        return new RoleManager<Role>(
                   roleStore,
                   new IRoleValidator<Role>[0],
                   new UpperInvariantLookupNormalizer(),
                   new Mock<IdentityErrorDescriber>().Object,
                   new Mock<ILogger<RoleManager<Role>>>(
                   ).Object);
    }

    public static UserManager<User> GetUserManager(WorkoutDbContext db)
    {
        var userStore = new UserStore<User>(db);
        //return new UserManager<User>(
        //    userStore,
        //    new Mock<IOptions<IdentityOptions>>().Object,
        //    new Mock<IPasswordHasher<User>>().Object,
        //    new IUserValidator<User>[0],
        //    new IPasswordValidator<User>[0],
        //    new UpperInvariantLookupNormalizer(),
        //    new Mock<IdentityErrorDescriber>().Object,
        //    new Mock<IServiceProvider>().Object,
        //    new Mock<ILogger<UserManager<User>>>().Object);
        return new UserManager<User>(
                    userStore,
                    new Mock<IOptions<IdentityOptions>>().Object,
                    new Mock<IPasswordHasher<User>>().Object,
                    new IUserValidator<User>[0],
                    new IPasswordValidator<User>[0],
                    new UpperInvariantLookupNormalizer(),
                    new Mock<IdentityErrorDescriber>().Object,
                    new Mock<IServiceProvider>().Object,
                    new Mock<ILogger<UserManager<User>>>(
                    ).Object);
    }
}