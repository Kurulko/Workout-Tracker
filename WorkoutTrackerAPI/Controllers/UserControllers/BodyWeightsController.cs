using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.BodyWeightServices;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;
using WorkoutTrackerAPI.Services.ExerciseServices;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public class BodyWeightsController : DbModelController<BodyWeight>
{
    readonly IHttpContextAccessor httpContextAccessor;
    readonly IBodyWeightService bodyWeightService;
    public BodyWeightsController(IBodyWeightService bodyWeightService, IHttpContextAccessor httpContextAccessor)
    {
        this.bodyWeightService = bodyWeightService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<BodyWeight> HandleBodyWeightServiceResult(ServiceResult<BodyWeight> serviceResult)
        => HandleServiceResult(serviceResult, "Body weight not found.");

    ActionResult InvalidBodyWeightID()
        => InvalidEntryID(nameof(BodyWeight));
    ActionResult BodyWeightIsNull()
        => EntryIsNull("Body weight");

    [HttpGet]
    public async Task<ActionResult<ApiResult<BodyWeight>>> GetCurrentUserBodyWeightsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetUserBodyWeightsAsync(userId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is null)
            return EntryNotFound("Body weights");

        return await ApiResult<BodyWeight>.CreateAsync(
            serviceResult.Model,
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{bodyWeightId}")]
    public async Task<ActionResult<BodyWeight>> GetCurrentUserBodyWeightByIdAsync(long bodyWeightId)
    {
        if (bodyWeightId < 1)
            return InvalidBodyWeightID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetUserBodyWeightByIdAsync(userId, bodyWeightId);
        return HandleBodyWeightServiceResult(serviceResult);
    }

    [HttpGet("by-date")]
    public async Task<ActionResult<BodyWeight>> GetCurrentUserBodyWeightByDateAsync(DateTime date)
    {
        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetUserBodyWeightByDateAsync(userId, DateOnly.FromDateTime(date));
        return HandleBodyWeightServiceResult(serviceResult);
    }

    [HttpGet("min-body-weight")]
    public async Task<ActionResult<BodyWeight>> GetMinCurrentUserBodyWeightAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetMinUserBodyWeightAsync(userId);
        return HandleBodyWeightServiceResult(serviceResult);
    }

    [HttpGet("max-body-weight")]
    public async Task<ActionResult<BodyWeight>> GetMaxCurrentUserBodyWeightAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetMaxUserBodyWeightAsync(userId);
        return HandleBodyWeightServiceResult(serviceResult);
    }

    [HttpPost]
    public async Task<ActionResult<BodyWeight>> AddBodyWeightToCurrentUserAsync(BodyWeight bodyWeight)
    {
        if (bodyWeight is null)
            return BodyWeightIsNull();

        if (bodyWeight.Id != 0)
            return InvalidEntryIDWhileAdding(nameof(BodyWeight), "body weight");

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.AddBodyWeightToUserAsync(userId, bodyWeight);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        bodyWeight = serviceResult.Model!;

        return CreatedAtAction(nameof(GetCurrentUserBodyWeightByIdAsync), new { id = bodyWeight.Id }, bodyWeight);
    }

    [HttpPut("{bodyWeightId}")]
    public async Task<IActionResult> UpdateCurrentUserBodyWeightAsync(long bodyWeightId, BodyWeight bodyWeight)
    {
        if (bodyWeightId < 1)
            return InvalidBodyWeightID();

        if (bodyWeight is null)
            return BodyWeightIsNull();

        if (bodyWeightId != bodyWeight.Id)
            return EntryIDsNotMatch(nameof(BodyWeight));

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.UpdateUserBodyWeightAsync(userId, bodyWeight);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{bodyWeightId}")]
    public async Task<IActionResult> DeleteBodyWeightFromCurrentUserAsync(long bodyWeightId)
    {
        if (bodyWeightId < 1)
            return InvalidBodyWeightID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.DeleteBodyWeightFromUserAsync(userId, bodyWeightId);
        return HandleServiceResult(serviceResult);
    }
}
