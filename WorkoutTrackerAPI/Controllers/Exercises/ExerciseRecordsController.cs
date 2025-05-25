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
    public async Task<ActionResult<ApiResult<ExerciseRecordDTO>>> GetCurrentUserExerciseRecordsAsync(
        long? exerciseId = null,
        ExerciseType? exerciseType = null,
        DateTimeRange? range = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (range is DateTimeRange _range && IsDateInFuture(_range))
            return DateInFuture();

        if (exerciseId.HasValue && !IsValidID(exerciseId.Value))
            return InvalidExerciseID();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecords = await exerciseRecordService.GetUserExerciseRecordsAsync(userId, exerciseId, exerciseType, range);

        var exerciseRecordDTOs = exerciseRecords.ToList().Select(mapper.Map<ExerciseRecordDTO>);
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
        if (!IsValidID(exerciseRecordId))
            return InvalidExerciseRecordID();

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecord = await exerciseRecordService.GetUserExerciseRecordByIdAsync(userId, exerciseRecordId);
        return ToExerciseRecordDTO(exerciseRecord);
    }

    [HttpPost("{exerciseRecordGroupId}")]
    public async Task<IActionResult> AddExerciseRecordToExerciseRecordGroupAsync(long exerciseRecordGroupId, ExerciseRecordCreationDTO exerciseRecordCreationDTO)
    {
        if (!IsValidID(exerciseRecordGroupId))
            return InvalidEntryID(nameof(ExerciseRecordGroup));

        if (exerciseRecordCreationDTO is null)
            return ExerciseRecordIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecord = mapper.Map<ExerciseRecord>(exerciseRecordCreationDTO);
        exerciseRecord = await exerciseRecordService.AddExerciseRecordToExerciseRecordGroupAsync(exerciseRecordGroupId, userId, exerciseRecord);

        var exerciseRecordDTO = mapper.Map<ExerciseRecordDTO>(exerciseRecord);
        return CreatedAtAction(nameof(GetCurrentUserExerciseRecordByIdAsync), new { exerciseRecordId = exerciseRecord.Id }, exerciseRecordDTO);
    }

    [HttpPut("{exerciseRecordId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseRecordAsync(long exerciseRecordId, ExerciseRecordUpdateDTO exerciseRecordDTO)
    {
        if (!IsValidID(exerciseRecordId))
            return InvalidExerciseRecordID();

        if (exerciseRecordDTO is null)
            return ExerciseRecordIsNull();

        if (!AreIdsEqual(exerciseRecordId, exerciseRecordDTO.Id))
            return EntryIDsNotMatch(nameof(ExerciseRecord));

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseRecord = mapper.Map<ExerciseRecord>(exerciseRecordDTO);
        await exerciseRecordService.UpdateUserExerciseRecordAsync(userId, exerciseRecord);

        return Ok();
    }

    [HttpDelete("{exerciseRecordId}")]
    public async Task<IActionResult> DeleteExerciseRecordFromCurrentUserAsync(long exerciseRecordId)
    {
        if (!IsValidID(exerciseRecordId))
            return InvalidExerciseRecordID();

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseRecordService.DeleteExerciseRecordFromUserAsync(userId, exerciseRecordId);
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