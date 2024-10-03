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
    readonly UserManager<User> userManager;
    readonly SignInManager<User> signInManager;
    readonly UserRepository userRepository;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly JwtHandler jwtHandler;

    public AccountService(SignInManager<User> signInManager, UserRepository userRepository, JwtHandler jwtHandler, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        this.signInManager = signInManager;
        this.userRepository = userRepository;
        this.jwtHandler = jwtHandler;
        this.httpContextAccessor = httpContextAccessor;
        this.userManager = userManager;
    }

    public async Task<AuthResult> LoginAsync(LoginModel login)
    {
        if (login is null)
            throw new EntryNullException("Login");

        var res = await signInManager.PasswordSignInAsync(login.Name, login.Password, login.RememberMe, false);

        if (!res.Succeeded)
        {
            return AuthResult.Fail("Password or/and login invalid");
        }

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
        string IdentityErrorsToString(IEnumerable<IdentityError> identityErrors)
            => string.Join("; ", identityErrors.Select(e => e.Description));


        if (register is null)
            throw new EntryNullException("Register");

        var existingUserByName = await userManager.FindByNameAsync(register.Name);
        if (existingUserByName is not null)
            return AuthResult.Fail("Name already registered.");

        var existingUserByEmail = await userManager.FindByEmailAsync(register.Email);
        if (existingUserByEmail is not null)
            return AuthResult.Fail("Email already registered.");


        User user = (User)register;
        IdentityResult result = await userRepository.CreateUserAsync(user, register.Password);

        if (!result.Succeeded)
        {
            string failMessage = IdentityErrorsToString(result.Errors);
            return AuthResult.Fail($"Register failed: {failMessage}");
        }

        try
        {
            user.Registered = DateTime.Now;
            await signInManager.SignInAsync(user, register.RememberMe);

            string userRole = Roles.UserRole;
            var identityResult = await userRepository.AddRolesToUserAsync(user.Id, new[] { userRole });
            if (!identityResult.Succeeded)
            {
                string failMessage = IdentityErrorsToString(result.Errors);
                return AuthResult.Fail($"Register failed: {failMessage}");
            }

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
