using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.Interfaces.Services.Auth;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Infrastructure.Auth;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Infrastructure.Extensions;

namespace WorkoutTracker.Infrastructure.Services.Auth;

internal class AccountService : IAccountService
{
    readonly SignInManager<User> signInManager;
    readonly IUserRepository userRepository;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly JwtHandler jwtHandler;

    public AccountService(
        SignInManager<User> signInManager, 
        IUserRepository userRepository, 
        JwtHandler jwtHandler, 
        IHttpContextAccessor httpContextAccessor)
    {
        this.signInManager = signInManager;
        this.userRepository = userRepository;
        this.jwtHandler = jwtHandler;
        this.httpContextAccessor = httpContextAccessor;
    }

    public virtual async Task<AuthResult> LoginAsync(LoginModel login)
    {
        if (login is null)
            throw new EntryNullException("Login");

        var res = await signInManager.PasswordSignInAsync(login.Name, login.Password, login.RememberMe, false);

        if (!res.Succeeded)
            return AuthResult.Fail("Password or/and login invalid");

        try
        {
            var user = await userRepository.GetUserByUsernameAsync(login.Name);
            var token = await jwtHandler.GenerateJwtTokenAsync(user!);
            return AuthResult.Ok("Login successful", token);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Login failed: {ex.Message}");
        }
    }


    public virtual async Task<AuthResult> RegisterAsync(RegisterModel register)
    {
        if (register is null)
            throw new EntryNullException("Register");

        var existingUserByName = await userRepository.GetUserByUsernameAsync(register.Name);
        if (existingUserByName is not null)
            return AuthResult.Fail("Name already registered.");

        if (!string.IsNullOrEmpty(register.Email))
        {
            var existingUserByEmail = await userRepository.GetUserByEmailAsync(register.Email!);
            if (existingUserByEmail is not null)
                return AuthResult.Fail("Email already registered.");
        }

        try
        {
            static string IdentityErrorsToString(IEnumerable<IdentityError> identityErrors)
                => string.Join("; ", identityErrors.Select(e => e.Description));

            User user = register.ToUser();
            user.Registered = DateTime.Now;

            IdentityResult result = await userRepository.CreateUserAsync(user, register.Password);
            if (!result.Succeeded)
                throw new Exception(IdentityErrorsToString(result.Errors));

            await signInManager.SignInAsync(user, register.RememberMe);

            string userRole = Roles.UserRole;

            var identityResult = await userRepository.AddRolesToUserAsync(user.Id, new[] { userRole });
            if (!identityResult.Succeeded)
                throw new Exception(IdentityErrorsToString(identityResult.Errors));

            var token = await jwtHandler.GenerateJwtTokenAsync(user);
            return AuthResult.Ok("Register successful", token);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Register failed: {ex.Message}");
        }
    }

    public virtual async Task LogoutAsync()
        => await signInManager.SignOutAsync();

    public virtual async Task<TokenModel> GetTokenAsync(string userId)
    {
        var user = await userRepository.GetUserByIdAsync(userId);

        return await jwtHandler.GenerateJwtTokenAsync(user!);
    }
}
