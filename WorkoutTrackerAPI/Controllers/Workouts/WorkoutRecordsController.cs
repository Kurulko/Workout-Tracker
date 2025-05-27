using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.DTOs.Workouts.WorkoutRecords;
using WorkoutTracker.Application.Interfaces.Services.Workouts;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.API.Controllers.UserControllers;

[Route("api/workout-records")]
public class WorkoutRecordsController : DbModelController<WorkoutRecordDTO, WorkoutRecordDTO>
{
    readonly IHttpContextAccessor httpContextAccessor;
    readonly IWorkoutRecordService workoutRecordService;
    public WorkoutRecordsController(
        IWorkoutRecordService workoutRecordService, 
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.workoutRecordService = workoutRecordService;
        this.httpContextAccessor = httpContextAccessor;
    }


    [HttpGet]
    public async Task<ActionResult<ApiResult<WorkoutRecordDTO>>> GetCurrentUserWorkoutRecordsAsync(CancellationToken cancellationToken,
        [FromQuery] long? workoutId,
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

        if (workoutId.HasValue && !IsValidID(workoutId.Value))
            return InvalidWorkoutID();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var workoutRecords = await workoutRecordService.GetUserWorkoutRecordsAsync(userId, workoutId, range, cancellationToken);

        var workoutRecordDTOs = workoutRecords.Select(mapper.Map<WorkoutRecordDTO>);
        return await ApiResult<WorkoutRecordDTO>.CreateAsync(
            workoutRecordDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }


    [HttpGet("{workoutRecordId}")]
    [ActionName(nameof(GetCurrentUserWorkoutRecordByIdAsync))]
    public async Task<ActionResult<WorkoutRecordDTO>> GetCurrentUserWorkoutRecordByIdAsync(long workoutRecordId, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutRecordId))
            return InvalidWorkoutRecordID();

        string userId = httpContextAccessor.GetUserId()!;
        var workoutRecord = await workoutRecordService.GetUserWorkoutRecordByIdAsync(userId, workoutRecordId, cancellationToken);
        return ToWorkoutRecordDTO(workoutRecord);
    }

    [HttpPost]
    public async Task<IActionResult> AddWorkoutRecordToCurrentUserAsync([FromBody] WorkoutRecordCreationDTO workoutRecordCreationDTO, CancellationToken cancellationToken)
    {
        if (workoutRecordCreationDTO is null)
            return WorkoutRecordIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var workoutRecord = mapper.Map<WorkoutRecord>(workoutRecordCreationDTO);
        workoutRecord = await workoutRecordService.AddWorkoutRecordToUserAsync(userId, workoutRecord, cancellationToken);

        var workoutRecordDTO = mapper.Map<WorkoutRecordDTO>(workoutRecord);
        return CreatedAtAction(nameof(GetCurrentUserWorkoutRecordByIdAsync), new { workoutRecordId = workoutRecord.Id }, workoutRecordDTO);
    }

    [HttpPut("{workoutRecordId}")]
    public async Task<IActionResult> UpdateCurrentUserWorkoutRecordAsync(long workoutRecordId, [FromBody] WorkoutRecordCreationDTO workoutRecordDTO, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutRecordId))
            return InvalidWorkoutRecordID();

        if (workoutRecordDTO is null)
            return WorkoutRecordIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var workoutRecord = mapper.Map<WorkoutRecord>(workoutRecordDTO);
        workoutRecord.Id = workoutRecordId;

        await workoutRecordService.UpdateUserWorkoutRecordAsync(userId, workoutRecord, cancellationToken);
        return Ok();
    }

    [HttpDelete("{workoutRecordId}")]
    public async Task<IActionResult> DeleteWorkoutRecordFromCurrentUserAsync(long workoutRecordId, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutRecordId))
            return InvalidWorkoutRecordID();

        string userId = httpContextAccessor.GetUserId()!;
        await workoutRecordService.DeleteWorkoutRecordFromUserAsync(userId, workoutRecordId, cancellationToken);
        return Ok();
    }

    ActionResult<WorkoutRecordDTO> ToWorkoutRecordDTO(WorkoutRecord? workoutRecord)
        => ToDTO<WorkoutRecord, WorkoutRecordDTO>(workoutRecord, "Workout record not found.");

    ActionResult InvalidWorkoutRecordID()
        => InvalidEntryID(nameof(WorkoutRecord));
    ActionResult InvalidWorkoutID()
        => InvalidEntryID(nameof(Workout));
    ActionResult WorkoutRecordIsNull()
        => EntryIsNull("Workout record");
}
