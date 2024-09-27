using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Services.ImpersonationServices;

namespace WorkoutTrackerAPI.Controllers.AuthControllers;

public class ImpersonationController : APIController
{
    readonly IImpersonationService impersonationService;
    public ImpersonationController(IImpersonationService impersonationService)
        => this.impersonationService = impersonationService;


    [HttpPost("impersonate")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> ImpersonateAsync(string userId)
    {
       if(string.IsNullOrEmpty(userId))
           return BadRequest($"User ID is null or empty.");

        try
        {
            await impersonationService.ImpersonateAsync(userId);
            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("revert")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> RevertAsync()
    {
        try
        {
            await impersonationService.RevertAsync();
            return Ok();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("is-impersonating")]
    [Authorize(Roles = Roles.AdminRole)]
    public ActionResult<bool>  IsImpersonating()
        => impersonationService.IsImpersonating();
}