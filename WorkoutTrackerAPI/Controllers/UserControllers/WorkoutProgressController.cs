using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs.ProgressDTOs;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services.ProgressServices;

namespace WorkoutTrackerAPI.Controllers.UserControllers;

[Authorize]
[Route("api/workout-progress")]
public class WorkoutProgressController : APIController
{
    readonly IWorkoutProgressService workoutProgressService;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly IMapper mapper;
    public WorkoutProgressController(IWorkoutProgressService workoutProgressService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        this.workoutProgressService = workoutProgressService;
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
    }

    [HttpGet("current")]
    public async Task<ActionResult<CurrentUserProgressDTO>> CalculateCurrentUserProgressAsync()
    {
        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            var currentUserProgress = await workoutProgressService.CalculateCurrentUserProgressAsync(userId);
            var currentUserProgressDTO = mapper.Map<CurrentUserProgressDTO>(currentUserProgress);
            return currentUserProgressDTO;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("total")]
    public async Task<ActionResult<WorkoutProgressDTO>> CalculateWorkoutProgressByRangeAsync([FromQuery] DateTimeRange? range)
    {
        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            var workoutProgress = await workoutProgressService.CalculateWorkoutProgressAsync(userId, range);
            var workoutProgressDTO = mapper.Map<WorkoutProgressDTO>(workoutProgress);
            return workoutProgressDTO;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("total/by-month")]
    public async Task<ActionResult<WorkoutProgressDTO>> CalculateCurrentWorkoutProgressForMonthAsync([FromQuery] int year, [FromQuery] int month)
    {
        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            var workoutProgress = await workoutProgressService.CalculateWorkoutProgressForMonthAsync(userId, new YearMonth(year, month));
            var workoutProgressDTO = mapper.Map<WorkoutProgressDTO>(workoutProgress);
            return workoutProgressDTO;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("total/by-year")]
    public async Task<ActionResult<WorkoutProgressDTO>> CalculateCurrentWorkoutProgressForYearAsync([FromQuery] int year)
    {
        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            var workoutProgress = await workoutProgressService.CalculateWorkoutProgressForYearAsync(userId, year);
            var workoutProgressDTO = mapper.Map<WorkoutProgressDTO>(workoutProgress);
            return workoutProgressDTO;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
