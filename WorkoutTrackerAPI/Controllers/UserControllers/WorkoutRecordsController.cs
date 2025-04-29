using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services.WorkoutRecordServices;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Data.Models;
using System;

namespace WorkoutTrackerAPI.Controllers.UserControllers;

[Route("api/workout-records")]
public class WorkoutRecordsController : DbModelController<WorkoutRecordDTO, WorkoutRecordDTO>
{
    readonly IHttpContextAccessor httpContextAccessor;
    readonly IWorkoutRecordService workoutRecordService;
    public WorkoutRecordsController(IWorkoutRecordService workoutRecordService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(mapper)
    {
        this.workoutRecordService = workoutRecordService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<WorkoutRecordDTO> HandleWorkoutRecordDTOServiceResult(ServiceResult<WorkoutRecord> serviceResult)
        => HandleDTOServiceResult<WorkoutRecord, WorkoutRecordDTO>(serviceResult, "Workout record not found.");

    ActionResult InvalidWorkoutRecordID()
        => InvalidEntryID(nameof(WorkoutRecord));
    ActionResult WorkoutRecordIsNull()
        => EntryIsNull("Workout record");

    [HttpGet]
    public async Task<ActionResult<ApiResult<WorkoutRecordDTO>>> GetCurrentUserWorkoutRecordsAsync(
        [FromQuery] long? workoutId,
        [FromQuery] DateTimeRange? range = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (workoutId.HasValue && workoutId < 1)
            return InvalidEntryID(nameof(Workout));

        if (range is not null && range.LastDate.Date > DateTime.Now.Date)
            return BadRequest("Incorrect date.");

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutRecordService.GetUserWorkoutRecordsAsync(userId, workoutId, range);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<WorkoutRecord> workoutRecords)
            return EntryNotFound("Workout records");

        var workoutRecordDTOs = workoutRecords.ToList().Select(m => mapper.Map<WorkoutRecordDTO>(m));
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
    public async Task<ActionResult<WorkoutRecordDTO>> GetCurrentUserWorkoutRecordByIdAsync(long workoutRecordId)
    {
        if (workoutRecordId < 1)
            return InvalidWorkoutRecordID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutRecordService.GetUserWorkoutRecordByIdAsync(userId, workoutRecordId);
        return HandleWorkoutRecordDTOServiceResult(serviceResult);
    }

    [HttpPost]
    public async Task<IActionResult> AddWorkoutRecordToCurrentUserAsync([FromBody] WorkoutRecordCreationDTO workoutRecordCreationDTO)
    {
        if (workoutRecordCreationDTO is null)
            return WorkoutRecordIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var workoutRecord = mapper.Map<WorkoutRecord>(workoutRecordCreationDTO);
        var serviceResult = await workoutRecordService.AddWorkoutRecordToUserAsync(userId, workoutRecord);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        workoutRecord = serviceResult.Model!;

        var workoutRecordDTO = mapper.Map<WorkoutRecordDTO>(workoutRecord);
        return CreatedAtAction(nameof(GetCurrentUserWorkoutRecordByIdAsync), new { workoutRecordId = workoutRecord.Id }, workoutRecordDTO);
    }

    [HttpPut("{workoutRecordId}")]
    public async Task<IActionResult> UpdateCurrentUserWorkoutRecordAsync(long workoutRecordId, [FromBody] WorkoutRecordCreationDTO workoutRecordDTO)
    {
        if (workoutRecordId < 1)
            return InvalidWorkoutRecordID();

        if (workoutRecordDTO is null)
            return WorkoutRecordIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var workoutRecord = mapper.Map<WorkoutRecord>(workoutRecordDTO);
        workoutRecord.Id = workoutRecordId;
        var serviceResult = await workoutRecordService.UpdateUserWorkoutRecordAsync(userId, workoutRecord);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{workoutRecordId}")]
    public async Task<IActionResult> DeleteWorkoutRecordFromCurrentUserAsync(long workoutRecordId)
    {
        if (workoutRecordId < 1)
            return InvalidWorkoutRecordID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutRecordService.DeleteWorkoutRecordFromUserAsync(userId, workoutRecordId);
        return HandleServiceResult(serviceResult);
    }
}
