using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Net.Mail;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.UserServices;

namespace WorkoutTrackerAPI.Services.AccountServices;

public class AccountService : IAccountService
{
    readonly UserManager<User> userManager;
    readonly SignInManager<User> signInManager;
    readonly IUserService userService;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly JwtHandler jwtHandler;

    public AccountService(SignInManager<User> signInManager, IUserService userService, JwtHandler jwtHandler, IHttpContextAccessor httpContextAccessor, UserManager<User> userManager)
    {
        this.signInManager = signInManager;
        this.userService = userService;
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
        IdentityResult result = await userService.CreateUserAsync(user, register.Password);

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
            var identityResult = await userService.AddRoleToUserAsync(user.Id, userRole);
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
        User user = (await userService.GetUserByClaimsAsync(claims))!;
        return await jwtHandler.GenerateJwtTokenAsync(user);
    }
}
