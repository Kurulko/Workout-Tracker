using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net.Mail;
using System.Security.Claims;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.UserServices;

namespace WorkoutTrackerAPI.Services.AccountServices;

public class AccountService : IAccountService
{
    readonly SignInManager<User> signInManager;
    readonly UserRepository userRepository;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly JwtHandler jwtHandler;

    public AccountService(SignInManager<User> signInManager, UserRepository userRepository, JwtHandler jwtHandler, IHttpContextAccessor httpContextAccessor)
    {
        this.signInManager = signInManager;
        this.userRepository = userRepository;
        this.jwtHandler = jwtHandler;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<AuthResult> LoginAsync(LoginModel login)
    {
        if (login is null)
            throw new EntryNullException("Login");

        var res = await signInManager.PasswordSignInAsync(login.Name, login.Password, login.RememberMe, false);

        if (!res.Succeeded)
            return AuthResult.Fail("Password or/and login invalid");

        try
        {
            var token = await jwtHandler.GenerateJwtTokenAsync((User)login);
            return AuthResult.Ok("Login successful", token);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Login failed: {ex.Message}");
        }
    }


    public async Task<AuthResult> RegisterAsync(RegisterModel register)
    {
        if (register is null)
            throw new EntryNullException("Register");

        var existingUserByName = await userRepository.GetUserByUsernameAsync(register.Name);
        if (existingUserByName is not null)
            return AuthResult.Fail("Name already registered.");

        var existingUserByEmail = await userRepository.GetUserByEmailAsync(register.Email!);
        if (existingUserByEmail is not null)
            return AuthResult.Fail("Email already registered.");


        try
        {
            string IdentityErrorsToString(IEnumerable<IdentityError> identityErrors)
                => string.Join("; ", identityErrors.Select(e => e.Description));

            User user = (User)register;
            user.Registered = DateTime.Now;

            IdentityResult result = await userRepository.CreateUserAsync(user, register.Password);
            if (!result.Succeeded)
                throw new Exception(IdentityErrorsToString(result.Errors));

            await signInManager.SignInAsync(user, register.RememberMe);

            string userRole = Roles.UserRole;

            var identityResult = await userRepository.AddRolesToUserAsync(user.Id, new[] { userRole });
            if (!identityResult.Succeeded)
                throw new Exception(IdentityErrorsToString(identityResult.Errors));

            var token = await jwtHandler.GenerateJwtTokenAsync((User)register);
            return AuthResult.Ok("Register successful", token);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"Register failed: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
        => await signInManager.SignOutAsync();

    public async Task<TokenModel> GetTokenAsync()
    {
        var claims = httpContextAccessor.HttpContext!.User;
        string userName = claims.Identity?.Name!;
        User user = (await userRepository.GetUserByUsernameAsync(userName))!;

        return await jwtHandler.GenerateJwtTokenAsync(user);
    }
}
