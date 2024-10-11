using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;
using Microsoft.AspNetCore.Authorization;
using System.Data;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Controllers.UserControllers;

public class ExerciseRecordsController : DbModelController<ExerciseRecord>
{
    readonly IExerciseRecordService exerciseRecordService;
    readonly IHttpContextAccessor httpContextAccessor;
    public ExerciseRecordsController(IExerciseRecordService exerciseRecordService, IHttpContextAccessor httpContextAccessor)
    {
        this.exerciseRecordService = exerciseRecordService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<ExerciseRecord> HandleExerciseRecordServiceResult(ServiceResult<ExerciseRecord> serviceResult)
        => HandleServiceResult(serviceResult, "Exercise record not found.");

    ActionResult InvalidExerciseRecordID()
        => InvalidEntryID(nameof(ExerciseRecord));
    ActionResult InvalidExerciseID()
        => InvalidEntryID(nameof(Exercise));
    ActionResult ExerciseRecordIsNull()
        => EntryIsNull("Exercise record");


    [HttpGet]
    public async Task<ActionResult<ApiResult<ExerciseRecord>>> GetCurrentUserExerciseRecordsAsync(
        long exerciseId,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseRecordService.GetUserExerciseRecordsAsync(userId, exerciseId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is null)
            return EntryNotFound("Exercise records");

        return await ApiResult<ExerciseRecord>.CreateAsync(
            serviceResult.Model!,
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{exerciseRecordId}")]
    public async Task<ActionResult<ExerciseRecord>> GetCurrentUserExerciseRecordByIdAsync(long exerciseRecordId)
    {
        if (exerciseRecordId < 1)
            return InvalidExerciseRecordID();
       
        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseRecordService.GetUserExerciseRecordByIdAsync(userId, exerciseRecordId);
        return HandleExerciseRecordServiceResult(serviceResult);
    }

    [HttpGet("by-date")]
    public async Task<ActionResult<ExerciseRecord>> GetCurrentUserExerciseRecordByDateAsync(long exerciseId, DateTime date)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseRecordService.GetUserExerciseRecordByDateAsync(userId, exerciseId, DateOnly.FromDateTime(date));
        return HandleExerciseRecordServiceResult(serviceResult);
    }


    [HttpPost]
    public async Task<ActionResult<ExerciseRecord>> AddExerciseRecordToCurrentUserAsync(ExerciseRecord exerciseRecord)
    {
        if (exerciseRecord is null)
            return ExerciseRecordIsNull();

        if (exerciseRecord.Id != 0)
            return InvalidEntryIDWhileAdding(nameof(ExerciseRecord), "exercise record");

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseRecordService.AddExerciseRecordToUserAsync(userId, exerciseRecord);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        exerciseRecord = serviceResult.Model!;

        return CreatedAtAction(nameof(GetCurrentUserExerciseRecordByIdAsync), new { id = exerciseRecord.Id }, exerciseRecord);
    }

    [HttpPut("{exerciseRecordId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseRecordAsync(long exerciseRecordId, ExerciseRecord exerciseRecord)
    {
        if (exerciseRecordId < 1)
            return InvalidExerciseRecordID();

        if (exerciseRecord is null)
            return ExerciseRecordIsNull();

        if (exerciseRecordId != exerciseRecord.Id)
            return EntryIDsNotMatch(nameof(ExerciseRecord));

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseRecordService.UpdateUserExerciseRecordAsync(userId, exerciseRecord);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{exerciseRecordId}")]
    public async Task<IActionResult> DeleteExerciseRecordFromCurrentUserAsync(long exerciseRecordId)
    {
        if (exerciseRecordId < 1)
            return InvalidExerciseRecordID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseRecordService.DeleteExerciseRecordFromUserAsync(userId, exerciseRecordId);
        return HandleServiceResult(serviceResult);
    }
}