﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.DTOs.Progresses;
using WorkoutTracker.Application.Interfaces.Services.Progresses;

namespace WorkoutTracker.API.Controllers;

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
    public async Task<ActionResult<CurrentUserProgressDTO>> CalculateCurrentUserProgressAsync(CancellationToken cancellationToken)
    {
        string userId = httpContextAccessor.GetUserId()!;
        var currentUserProgress = await workoutProgressService.CalculateCurrentUserProgressAsync(userId, cancellationToken);
        var currentUserProgressDTO = mapper.Map<CurrentUserProgressDTO>(currentUserProgress);
        return currentUserProgressDTO;
    }

    [HttpGet("total")]
    public async Task<ActionResult<WorkoutProgressDTO>> CalculateWorkoutProgressByRangeAsync([FromQuery] DateTimeRange? range, CancellationToken cancellationToken)
    {
        string userId = httpContextAccessor.GetUserId()!;
        var workoutProgress = await workoutProgressService.CalculateWorkoutProgressAsync(userId, range, cancellationToken);
        var workoutProgressDTO = mapper.Map<WorkoutProgressDTO>(workoutProgress);
        return workoutProgressDTO;
    }

    [HttpGet("total/by-month")]
    public async Task<ActionResult<WorkoutProgressDTO>> CalculateCurrentWorkoutProgressForMonthAsync([FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
    {
        string userId = httpContextAccessor.GetUserId()!;
        var workoutProgress = await workoutProgressService.CalculateWorkoutProgressForMonthAsync(userId, new YearMonth(year, month), cancellationToken);
        var workoutProgressDTO = mapper.Map<WorkoutProgressDTO>(workoutProgress);
        return workoutProgressDTO;
    }

    [HttpGet("total/by-year")]
    public async Task<ActionResult<WorkoutProgressDTO>> CalculateCurrentWorkoutProgressForYearAsync([FromQuery] int year, CancellationToken cancellationToken)
    {
        string userId = httpContextAccessor.GetUserId()!;
        var workoutProgress = await workoutProgressService.CalculateWorkoutProgressForYearAsync(userId, year, cancellationToken);
        var workoutProgressDTO = mapper.Map<WorkoutProgressDTO>(workoutProgress);
        return workoutProgressDTO;
    }
}
