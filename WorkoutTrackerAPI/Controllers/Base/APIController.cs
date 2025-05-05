using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Results;

namespace WorkoutTracker.API.Controllers.Base;

[ApiController]
[Route($"api/[controller]")]
public abstract class APIController : Controller
{
    protected ActionResult InvalidPageIndexOrPageSize()
        => BadRequest("Invalid page index or page size.");
    protected ActionResult InvalidEntryID(string entryName)
        => BadRequest($"Invalid {entryName} ID.");
    protected ActionResult InvalidEntryIDWhileAdding(string entryName, string modelName)
        => BadRequest($"{entryName} ID must not be set when adding a new {modelName}.");
    protected ActionResult EntryIsNull(string entryName)
        => BadRequest($"{entryName} entry is null.");
    protected ActionResult EntryNotFound(string entryName)
        => NotFound($"{entryName} not found.");
    protected ActionResult EntryIDsNotMatch(string entryName)
        => BadRequest($"{entryName} IDs do not match.");

    protected ActionResult HandleException(Exception ex)
    {
        switch (ex)
        {
            case SecurityTokenException or UnauthorizedAccessException:
                return Unauthorized(ex.Message); // 401 Unauthorized

            case ArgumentException or InvalidOperationException:
                return BadRequest(ex.Message); // 400 Bad Request

            case NotFoundException or FileNotFoundException:
                return NotFound(ex.Message); // 404 Not Found

            default:
                // For any unexpected exception, return a 500 Internal Server Error
                return StatusCode(500, $"Internal server error: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    protected IActionResult HandleIdentityResult(IdentityResult identityResult)
    {
        if (identityResult.Succeeded)
            return Ok();

        return BadRequest(GetIdentityErrorsStr(identityResult.Errors));
    }

    protected IActionResult HandleServiceResult(ServiceResult serviceResult)
    {
        if (serviceResult.Success)
            return Ok();

        return BadRequest(serviceResult.ErrorMessage);
    }

    protected ActionResult<T> HandleServiceResult<T>(ServiceResult<T> serviceResult, string? notFoundMessage = null)
        where T : class
    {
        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is T model)
            return model;

        return NotFound(notFoundMessage);
    }

    string[] GetIdentityErrorsStr(IEnumerable<IdentityError> identityErrors)
        => identityErrors.Select(e => e.Description).ToArray();
}
