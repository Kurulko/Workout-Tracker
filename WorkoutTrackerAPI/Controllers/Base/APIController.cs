using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.DTOs;

namespace WorkoutTracker.API.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
public abstract class APIController : Controller
{
    protected bool IsValidPageIndexAndPageSize(int pageIndex, int pageSize)
        => pageIndex >= 0 && pageSize > 0;

    protected bool IsDateInFuture(DateTime date)
        => date > DateTime.UtcNow.Date;
    protected bool IsDateInFuture(DateTimeRange range)
        => IsDateInFuture(range.LastDate);
    protected bool IsDateInFuture(DateAndTime dateAndTime)
        => IsDateInFuture(dateAndTime.Date);

    protected ActionResult InvalidPageIndexOrPageSize()
        => BadRequest("Invalid page index or page size.");
    protected ActionResult DateInFuture()
        => BadRequest("Date cannot be in future.");
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
}
