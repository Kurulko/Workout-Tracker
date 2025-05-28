using Microsoft.AspNetCore.Mvc;
using System.Data;
using AutoMapper;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecords;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
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
    public ExerciseRecordsController(
        IExerciseRecordService exerciseRecordService,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.exerciseRecordService = exerciseRecordService;
        this.httpContextAccessor = httpContextAccessor;
    }


    [HttpGet]
    public async Task<ActionResult<ApiResult<ExerciseRecordDTO>>> GetCurrentUserExerciseRecordsAsync(CancellationToken cancellationToken,
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
        if (range is DateTimeRange _range && IsDateInFuture(_range))
            return DateInFuture();

        if (exerciseId.HasValue && !IsValidID(exerciseId.Value))
            return InvalidExerciseID();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecords = await exerciseRecordService.GetUserExerciseRecordsAsync(userId, exerciseId, exerciseType, range, cancellationToken);

        var exerciseRecordDTOs = exerciseRecords.Select(mapper.Map<ExerciseRecordDTO>);
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
    public async Task<ActionResult<ExerciseRecordDTO>> GetCurrentUserExerciseRecordByIdAsync(long exerciseRecordId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseRecordId))
            return InvalidExerciseRecordID();

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecord = await exerciseRecordService.GetUserExerciseRecordByIdAsync(userId, exerciseRecordId, cancellationToken);
        return ToExerciseRecordDTO(exerciseRecord);
    }

    [HttpPost("{exerciseRecordGroupId}")]
    public async Task<IActionResult> AddExerciseRecordToExerciseRecordGroupAsync(long exerciseRecordGroupId, ExerciseRecordCreationDTO exerciseRecordCreationDTO, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseRecordGroupId))
            return InvalidEntryID(nameof(ExerciseRecordGroup));

        if (exerciseRecordCreationDTO is null)
            return ExerciseRecordIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecord = mapper.Map<ExerciseRecord>(exerciseRecordCreationDTO);

        exerciseRecord = await exerciseRecordService.AddExerciseRecordToExerciseRecordGroupAsync(exerciseRecordGroupId, userId, exerciseRecord, cancellationToken);

        var exerciseRecordDTO = mapper.Map<ExerciseRecordDTO>(exerciseRecord);
        return CreatedAtAction(nameof(GetCurrentUserExerciseRecordByIdAsync), new { exerciseRecordId = exerciseRecord.Id }, exerciseRecordDTO);
    }

    [HttpPut("{exerciseRecordId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseRecordAsync(long exerciseRecordId, ExerciseRecordUpdateDTO exerciseRecordDTO, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseRecordId))
            return InvalidExerciseRecordID();

        if (exerciseRecordDTO is null)
            return ExerciseRecordIsNull();

        if (!AreIdsEqual(exerciseRecordId, exerciseRecordDTO.Id))
            return EntryIDsNotMatch(nameof(ExerciseRecord));

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecord = mapper.Map<ExerciseRecord>(exerciseRecordDTO);
        await exerciseRecordService.UpdateUserExerciseRecordAsync(userId, exerciseRecord, cancellationToken);

        return Ok();
    }

    [HttpDelete("{exerciseRecordId}")]
    public async Task<IActionResult> DeleteExerciseRecordFromCurrentUserAsync(long exerciseRecordId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseRecordId))
            return InvalidExerciseRecordID();

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseRecordService.DeleteExerciseRecordFromUserAsync(userId, exerciseRecordId, cancellationToken);
        return Ok();
    }


    ActionResult<ExerciseRecordDTO> ToExerciseRecordDTO(ExerciseRecord? exerciseRecord)
        => ToDTO<ExerciseRecord, ExerciseRecordDTO>(exerciseRecord, "Exercise record not found.");

    ActionResult InvalidExerciseRecordID()
        => InvalidEntryID(nameof(ExerciseRecord));
    ActionResult InvalidExerciseID()
        => InvalidEntryID(nameof(Exercise));
    ActionResult ExerciseRecordIsNull()
        => EntryIsNull("Exercise record");
}