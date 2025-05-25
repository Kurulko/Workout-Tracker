using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.Interfaces.Services.Auth;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Infrastructure.Auth;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Extensions;
using Microsoft.Extensions.Logging;
using WorkoutTracker.Infrastructure.Validators.Services.Auth;

namespace WorkoutTracker.Infrastructure.Services.Auth;

internal class AccountService : IAccountService
{
    readonly SignInManager<User> signInManager;
    readonly IUserRepository userRepository;
    readonly JwtHandler jwtHandler;
    readonly AccountServiceValidator accountServiceValidator;
    readonly ILogger<AccountService> logger;

    public AccountService(
        SignInManager<User> signInManager, 
        IUserRepository userRepository, 
        JwtHandler jwtHandler,
        AccountServiceValidator accountServiceValidator,
        ILogger<AccountService> logger
    )
    {
        this.signInManager = signInManager;
        this.userRepository = userRepository;
        this.jwtHandler = jwtHandler;
        this.accountServiceValidator = accountServiceValidator;
        this.logger = logger;
    }


    public virtual async Task<AuthResult> LoginAsync(LoginModel login)
    {
        await accountServiceValidator.ValidateLoginAsync(login);

        try
        {
            var res = await signInManager.PasswordSignInAsync(login.Name, login.Password, login.RememberMe, false);

            if (!res.Succeeded)
                return AuthResult.Fail("Password or/and login invalid");

            var user = await userRepository.GetUserByUsernameAsync(login.Name);
            var token = await jwtHandler.GenerateJwtTokenAsync(user!);
            return AuthResult.Ok("Login successful", token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Login failed: {ex.Message}");
            return AuthResult.Fail("Login failed");
        }
    }


    public virtual async Task<AuthResult> RegisterAsync(RegisterModel register)
    {
        await accountServiceValidator.ValidateRegisterAsync(register);

        try
        {
            User user = register.ToUser();
            user.Registered = DateTime.Now;

            await userRepository.CreateUserAsync(user, register.Password);
            await signInManager.SignInAsync(user, register.RememberMe);

            string userRole = Roles.UserRole;
            await userRepository.AddRolesToUserAsync(user.Id, [ userRole ]);

            var token = await jwtHandler.GenerateJwtTokenAsync(user);
            return AuthResult.Ok("Register successful", token);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Register failed: {ex.Message}");
            return AuthResult.Fail("Register failed");
        }
    }

    public virtual async Task LogoutAsync()
    {
        await accountServiceValidator.ValidateLogoutAsync();

        await signInManager.SignOutAsync();
    }

    public virtual async Task<TokenModel> GetTokenAsync(string userId)
    {
        await accountServiceValidator.ValidateGetTokenAsync(userId);

        var user = await userRepository.GetUserByIdAsync(userId);
        return await jwtHandler.GenerateJwtTokenAsync(user!);
    }
}
