using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.WorkoutServices;
using WorkoutTrackerAPI.Exceptions;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Services.MuscleServices;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public class WorkoutsController : BaseWorkoutController<Workout>
{
    readonly IWorkoutService workoutService;
    readonly IHttpContextAccessor httpContextAccessor;
    public WorkoutsController(IWorkoutService workoutService, IHttpContextAccessor httpContextAccessor)
    {
        this.workoutService = workoutService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<Workout> HandleWorkoutServiceResult(ServiceResult<Workout> serviceResult)
        => HandleServiceResult(serviceResult, "Workout not found.");

    ActionResult InvalidWorkoutID()
        => InvalidEntryID(nameof(Workout));
    ActionResult WorkoutNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Workout));
    ActionResult WorkoutIsNull()
        => EntryIsNull(nameof(Workout));


    [HttpGet]
    public async Task<ActionResult<ApiResult<Workout>>> GetCurrentUserWorkoutsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.GetUserWorkoutsAsync(userId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is null)
            return EntryNotFound("Workouts");

        return await ApiResult<Workout>.CreateAsync(
            serviceResult.Model!,
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{workoutId}")]
    public async Task<ActionResult<Workout>> GetCurrentUserWorkoutByIdAsync(long workoutId)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.GetUserWorkoutByIdAsync(userId, workoutId);
        return HandleWorkoutServiceResult(serviceResult);
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<Workout>> GetCurrentUserWorkoutByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return WorkoutNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.GetUserWorkoutByNameAsync(userId, name);
        return HandleWorkoutServiceResult(serviceResult);
    }


    [HttpPost]
    public async Task<ActionResult<Workout>> AddCurrentUserWorkoutAsync(Workout workout)
    {
        if (workout is null)
            return WorkoutIsNull();

        if (workout.Id != 0)
            return InvalidEntryIDWhileAdding(nameof(Workout), "workout");

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.AddUserWorkoutAsync(userId, workout);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        workout = serviceResult.Model!;

        return CreatedAtAction(nameof(GetCurrentUserWorkoutByIdAsync), new { id = workout.Id }, workout);
    }

    [HttpPut("{workoutId}")]
    public async Task<IActionResult> UpdateCurrentUserWorkoutAsync(long workoutId, Workout workout)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        if (workout is null)
            return WorkoutIsNull();

        if (workoutId != workout.Id)
            return EntryIDsNotMatch(nameof(Workout));

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.UpdateUserWorkoutAsync(userId, workout);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{workoutId}")]
    public async Task<IActionResult> DeleteCurrentUserWorkoutAsync(long workoutId)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.DeleteUserWorkoutAsync(userId, workoutId);
        return HandleServiceResult(serviceResult);
    }

    [HttpGet("workout-exists/{workoutId}")]
    public async Task<ActionResult<bool>> CurrentUserWorkoutExistsAsync(long workoutId)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            return await workoutService.UserWorkoutExistsAsync(userId, workoutId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("workout-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> CurrentUserWorkoutExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return WorkoutNameIsNullOrEmpty();

        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            return await workoutService.UserWorkoutExistsByNameAsync(userId, name);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
