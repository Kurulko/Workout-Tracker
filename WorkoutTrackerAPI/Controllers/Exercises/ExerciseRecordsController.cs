using Microsoft.AspNetCore.Mvc;
using System.Data;
using AutoMapper;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.API.Results;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.API.Extensions;

namespace WorkoutTracker.API.Controllers.Exercises;

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
        => HandleDTOServiceResult<ExerciseRecord, ExerciseRecordDTO>(serviceResult, "Exercise record not found.");

    ActionResult InvalidExerciseRecordID()
        => InvalidEntryID(nameof(ExerciseRecord));
    ActionResult InvalidExerciseID()
        => InvalidEntryID(nameof(Exercise));
    ActionResult ExerciseRecordIsNull()
        => EntryIsNull("Exercise record");


    [HttpGet]
    public async Task<ActionResult<ApiResult<ExerciseRecordDTO>>> GetCurrentUserExerciseRecordsAsync(
        [FromQuery] long? exerciseId = null,
        [FromQuery] ExerciseType? exerciseType = null,
        [FromQuery] DateTimeRange? range = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (range is DateTimeRange _range && _range.LastDate > DateTime.Now.Date)
            return BadRequest("Incorrect date.");

        if (exerciseId.HasValue && exerciseId < 1)
            return InvalidExerciseID();

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseRecordService.GetUserExerciseRecordsAsync(userId, exerciseId, exerciseType, range);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<ExerciseRecord> exerciseRecords)
            return EntryNotFound("Exercise records");

        var exerciseRecordDTOs = exerciseRecords.ToList().Select(m => mapper.Map<ExerciseRecordDTO>(m));
        return await ApiResult<ExerciseRecordDTO>.CreateAsync(
            exerciseRecordDTOs.AsQueryable(),
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

        var exerciseRecordDTO = mapper.Map<ExerciseRecordDTO>(exerciseRecord);
        return CreatedAtAction(nameof(GetCurrentUserExerciseRecordByIdAsync), new { exerciseRecordId = exerciseRecord.Id }, exerciseRecordDTO);
    }

    [HttpPut("{exerciseRecordId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseRecordAsync(long exerciseRecordId, ExerciseRecordUpdateDTO exerciseRecordDTO)
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