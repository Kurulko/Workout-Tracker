using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Win32;
using Serilog;
using WorkoutTrackerAPI.Data.Account;
using WorkoutTrackerAPI.Services.AccountServices;

namespace WorkoutTrackerAPI.Controllers.AuthControllers;

public class AccountController : APIController
{
    readonly IAccountService accountService;
    public AccountController(IAccountService accountService)
        => this.accountService = accountService;


    [HttpPost("Register")]
    public async Task<IActionResult> RegisterAsync(RegisterModel register)
    {
        if (register is null)
            return EntryIsNull("Register");

        if (!ModelState.IsValid)
            return HandleInvalidModelState();

        var result = await accountService.RegisterAsync(register);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [HttpPost("Login")]
    public async Task<IActionResult> LoginAsync(LoginModel login)
    {
        if (login is null)
            return EntryIsNull("Login");

        if (!ModelState.IsValid)
            return HandleInvalidModelState();

        var result = await accountService.LoginAsync(login);
        return result.Success ? Ok(result) : Unauthorized(result);
    }

    [Authorize]
    [HttpGet("Token")]
    public async Task<IActionResult> GetTokenAsync()
    {
        try
        {
            var token = await accountService.GetTokenAsync();
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
