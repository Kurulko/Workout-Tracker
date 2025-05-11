using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.Interfaces.Services.Auth;

namespace WorkoutTracker.API.Controllers.AuthControllers;

public class AccountController : APIController
{
    readonly IAccountService accountService;
    readonly IHttpContextAccessor httpContextAccessor;
    public AccountController(IAccountService accountService, IHttpContextAccessor httpContextAccessor)
    {
        this.accountService = accountService;
        this.httpContextAccessor = httpContextAccessor;
    }

    IActionResult HandleAuthResult(AuthResult authResult)
        => authResult.Success ? Ok(authResult) : BadRequest(authResult);

    [HttpPost("register")]
    public async Task<IActionResult> RegisterAsync(RegisterModel register)
    {
        if (register is null)
            return EntryIsNull("Register");

        var result = await accountService.RegisterAsync(register);
        return HandleAuthResult(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync(LoginModel login)
    {
        if (login is null)
            return EntryIsNull("Login");

        var result = await accountService.LoginAsync(login);
        return HandleAuthResult(result);
    }

    [Authorize]
    [HttpGet("token")]
    public async Task<IActionResult> GetTokenAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        var token = await accountService.GetTokenAsync(userId);
        return Ok(token);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        await accountService.LogoutAsync();
        return Ok();
    }
}
