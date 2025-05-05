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

    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync(RegisterModel register)
    {
        if (register is null)
            return EntryIsNull("Register");

        var result = await accountService.RegisterAsync(register);
        return HandleAuthResult(result);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync(LoginModel login)
    {
        if (login is null)
            return EntryIsNull("Login");

        var result = await accountService.LoginAsync(login);
        return HandleAuthResult(result);
    }

    [Authorize]
    [HttpGet("Token")]
    public async Task<IActionResult> GetTokenAsync()
    {
        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            var token = await accountService.GetTokenAsync(userId);
            return Ok(token);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [Authorize]
    [HttpPost("Logout")]
    public async Task<IActionResult> LogoutAsync()
    {
        try
        {
            await accountService.LogoutAsync();
            return Ok();
        }
        catch(Exception ex) 
        {
            return BadRequest($"Logout failed: {ex.Message}.");
        }
    }
}
