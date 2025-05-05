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


    [HttpPost("impersonate/{userId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<ActionResult> ImpersonateAsync(string userId)
    {
       if(string.IsNullOrEmpty(userId))
           return BadRequest("User ID is null or empty.");

        try
        {
            var token = await impersonationService.ImpersonateAsync(userId);
            return Ok(token);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("revert")]
    [Authorize]
    public async Task<ActionResult> RevertAsync()
    {
        try
        {
            var token = await impersonationService.RevertAsync();
            return Ok(token);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("is-impersonating")]
    [Authorize]
    public ActionResult<bool>  IsImpersonating()
        => impersonationService.IsImpersonating();
}