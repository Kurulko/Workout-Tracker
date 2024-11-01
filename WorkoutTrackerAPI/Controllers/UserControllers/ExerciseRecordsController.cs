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
using WorkoutTrackerAPI.Data.DTOs;
using AutoMapper;

namespace WorkoutTrackerAPI.Controllers.UserControllers;

[Route("api/exercise-records")]
public class ExerciseRecordsController : DbModelController<ExerciseRecordDTO, ExerciseRecordCreationDTO>
{
    readonly IExerciseRecordService exerciseRecordService;
    readonly IHttpContextAccessor httpContextAccessor;
    public ExerciseRecordsController(IExerciseRecordService exerciseRecordService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(mapper)
    {
        this.exerciseRecordService = exerciseRecordService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<ExerciseRecordDTO> HandleExerciseRecordDTOServiceResult(ServiceResult<ExerciseRecord> serviceResult)
        => HandleDTOServiceResult(serviceResult, "Exercise record not found.");

    ActionResult InvalidExerciseRecordID()
        => InvalidEntryID(nameof(ExerciseRecord));
    ActionResult InvalidExerciseID()
        => InvalidEntryID(nameof(Exercise));
    ActionResult ExerciseRecordIsNull()
        => EntryIsNull("Exercise record");


    [HttpGet]
    public async Task<ActionResult<ApiResult<ExerciseRecordDTO>>> GetCurrentUserExerciseRecordsAsync(
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

        if (serviceResult.Model is not IQueryable<ExerciseRecord> exerciseRecords)
            return EntryNotFound("Exercise records");

        var exerciseRecordDTOs = exerciseRecords.Select(m => mapper.Map<ExerciseRecordDTO>(m));
        return await ApiResult<ExerciseRecordDTO>.CreateAsync(
            exerciseRecordDTOs,
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{exerciseRecordId}")]
    [ActionName(nameof(GetCurrentUserExerciseRecordByIdAsync))]
    public async Task<ActionResult<ExerciseRecordDTO>> GetCurrentUserExerciseRecordByIdAsync(long exerciseRecordId)
    {
        if (exerciseRecordId < 1)
            return InvalidExerciseRecordID();
       
        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseRecordService.GetUserExerciseRecordByIdAsync(userId, exerciseRecordId);
        return HandleExerciseRecordDTOServiceResult(serviceResult);
    }

    [HttpGet("by-date")]
    public async Task<ActionResult<ExerciseRecordDTO>> GetCurrentUserExerciseRecordByDateAsync(long exerciseId, DateTime date)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseRecordService.GetUserExerciseRecordByDateAsync(userId, exerciseId, DateOnly.FromDateTime(date));
        return HandleExerciseRecordDTOServiceResult(serviceResult);
    }


    [HttpPost]
    public async Task<IActionResult> AddExerciseRecordToCurrentUserAsync(ExerciseRecordCreationDTO exerciseRecordCreationDTO)
    {
        if (exerciseRecordCreationDTO is null)
            return ExerciseRecordIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecord = mapper.Map<ExerciseRecord>(exerciseRecordCreationDTO);
        var serviceResult = await exerciseRecordService.AddExerciseRecordToUserAsync(userId, exerciseRecord);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        exerciseRecord = serviceResult.Model!;

        return CreatedAtAction(nameof(GetCurrentUserExerciseRecordByIdAsync), new { id = exerciseRecord.Id }, exerciseRecord);
    }

    [HttpPut("{exerciseRecordId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseRecordAsync(long exerciseRecordId, ExerciseRecordDTO exerciseRecordDTO)
    {
        if (exerciseRecordId < 1)
            return InvalidExerciseRecordID();

        if (exerciseRecordDTO is null)
            return ExerciseRecordIsNull();

        if (exerciseRecordId != exerciseRecordDTO.Id)
            return EntryIDsNotMatch(nameof(ExerciseRecord));

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecord = mapper.Map<ExerciseRecord>(exerciseRecordDTO);
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