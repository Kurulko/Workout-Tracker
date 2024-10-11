using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.BodyWeightServices;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Services.MuscleSizeServices;
using WorkoutTrackerAPI.Services.WorkoutServices;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public class MuscleSizesController : DbModelController<MuscleSize>
{
    readonly IMuscleSizeService muscleSizeService;
    readonly IHttpContextAccessor httpContextAccessor;
    public MuscleSizesController(IMuscleSizeService muscleSizeService, IHttpContextAccessor httpContextAccessor)
    {
        this.muscleSizeService = muscleSizeService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<MuscleSize> HandleMuscleSizeServiceResult(ServiceResult<MuscleSize> serviceResult)
        => HandleServiceResult(serviceResult, "Muscle size not found.");

    ActionResult InvalidMuscleSizeID()
        => InvalidEntryID(nameof(MuscleSize));
    ActionResult InvalidMuscleID()
        => InvalidEntryID(nameof(Muscle));
    ActionResult MuscleSizeIsNull()
        => EntryIsNull("Muscle size");


    [HttpGet]
    public async Task<ActionResult<ApiResult<MuscleSize>>> GetCurrentUserMuscleSizesAsync(
        long muscleId,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (muscleId < 1)
            return InvalidEntryID(nameof(Muscle));

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetUserMuscleSizesAsync(userId, muscleId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is null)
            return EntryNotFound("Muscle sizes");

        return await ApiResult<MuscleSize>.CreateAsync(
            serviceResult.Model!,
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{muscleSizeId}")]
    public async Task<ActionResult<MuscleSize>> GetCurrentUserMuscleSizeByIdAsync(long muscleSizeId)
    {
        if (muscleSizeId < 1)
            return InvalidMuscleSizeID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetUserMuscleSizeByIdAsync(userId, muscleSizeId);
        return HandleMuscleSizeServiceResult(serviceResult);
    }

    [HttpGet("by-date")]
    public async Task<ActionResult<MuscleSize>> GetCurrentUserMuscleSizeByDateAsync(long muscleId, DateTime date)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetUserMuscleSizeByDateAsync(userId, muscleId, DateOnly.FromDateTime(date));
        return HandleMuscleSizeServiceResult(serviceResult);
    }

    [HttpGet("min-muscle-size")]
    public async Task<ActionResult<MuscleSize>> GetMinCurrentUserMuscleSizeAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetMinUserMuscleSizeAsync(userId, muscleId);
        return HandleMuscleSizeServiceResult(serviceResult);
    }

    [HttpGet("max-muscle-size")]
    public async Task<ActionResult<MuscleSize>> GetMaxCurrentUserMuscleSizeAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetMaxUserMuscleSizeAsync(userId, muscleId);
        return HandleMuscleSizeServiceResult(serviceResult);
    }


    [HttpPost]
    public async Task<ActionResult<MuscleSize>> AddMuscleSizeToCurrentUserAsync(MuscleSize muscleSize)
    {
        if (muscleSize is null)
            return MuscleSizeIsNull();

        if (muscleSize.Id != 0)
            return InvalidEntryIDWhileAdding(nameof(MuscleSize), "muscle size");

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.AddMuscleSizeToUserAsync(userId, muscleSize);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        muscleSize = serviceResult.Model!;

        return CreatedAtAction(nameof(GetCurrentUserMuscleSizeByIdAsync), new { id = muscleSize.Id }, muscleSize);
    }

    [HttpPut("{muscleSizeId}")]
    public async Task<IActionResult> UpdateCurrentUserMuscleSizeAsync(long muscleSizeId, MuscleSize muscleSize)
    {
        if(muscleSizeId < 1)
            return InvalidMuscleSizeID();

        if (muscleSize is null)
            return MuscleSizeIsNull();

        if (muscleSizeId != muscleSize.Id)
            return EntryIDsNotMatch(nameof(MuscleSize));

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.UpdateUserMuscleSizeAsync(userId, muscleSize);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{muscleSizeId}")]
    public async Task<IActionResult> DeleteMuscleSizeFromCurrentUserAsync(long muscleSizeId)
    {
        if (muscleSizeId < 1)
            return InvalidMuscleSizeID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.DeleteMuscleSizeFromUserAsync(userId, muscleSizeId);
        return HandleServiceResult(serviceResult);
    }
}
