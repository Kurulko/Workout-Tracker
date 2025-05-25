using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.Application.Interfaces.Services.Auth;
using WorkoutTracker.Domain.Constants;

namespace WorkoutTracker.API.Controllers.AuthControllers;

public class ImpersonationController : APIController
{
    readonly IImpersonationService impersonationService;
    public ImpersonationController(IImpersonationService impersonationService)
        => this.impersonationService = impersonationService;


    [Authorize(Roles = Roles.AdminRole)]
    [HttpPost("impersonate/{userId}")]
    public async Task<ActionResult> ImpersonateAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest("User ID is null or empty.");

        var token = await impersonationService.ImpersonateAsync(userId);
        return Ok(token);
    }

    [Authorize]
    [HttpPost("revert")]
    public async Task<ActionResult> RevertAsync()
    {
        var token = await impersonationService.RevertAsync();
        return Ok(token);
    }

    [Authorize]
    [HttpGet("is-impersonating")]
    public ActionResult<bool> IsImpersonating()
    {
        return impersonationService.IsImpersonating();
    }
}