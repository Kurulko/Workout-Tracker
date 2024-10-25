using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Services.WorkoutServices;

namespace ExerciseTrackerAPI.Controllers.ExerciseControllers;

public class ExercisesController : BaseWorkoutController<Exercise>
{
    readonly IExerciseService exerciseService;
    readonly IHttpContextAccessor httpContextAccessor;
    public ExercisesController(IExerciseService exerciseService, IHttpContextAccessor httpContextAccessor)
    {
        this.exerciseService = exerciseService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<Exercise> HandleExerciseServiceResult(ServiceResult<Exercise> serviceResult)
        => HandleServiceResult(serviceResult, "Exercise not found.");

    ActionResult InvalidExerciseID()
        => InvalidEntryID(nameof(Exercise));
    ActionResult ExerciseNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Exercise));
    ActionResult ExerciseIsNull()
        => EntryIsNull(nameof(Exercise));
    ActionResult ExerciseIDsNotMatch()
        => EntryIDsNotMatch(nameof(Exercise));
    ActionResult InvalidExerciseIDWhileAdding()
        => InvalidEntryIDWhileAdding(nameof(Exercise), "exercise");

    [HttpGet("exercises")]
    public async Task<ActionResult<ApiResult<Exercise>>> GetExercisesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await exerciseService.GetExercisesAsync();

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is null)
            return EntryNotFound("Exercises");

        return await ApiResult<Exercise>.CreateAsync(
            serviceResult.Model!,
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("user-exercises")]
    public async Task<ActionResult<ApiResult<Exercise>>> GetCurrentUserExercisesAsync(
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
        var serviceResult = await exerciseService.GetUserExercisesAsync(userId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is null)
            return EntryNotFound("User exercises");

        return await ApiResult<Exercise>.CreateAsync(
            serviceResult.Model!,
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("exercise/{exerciseId}")]
    [ActionName(nameof(GetExerciseByIdAsync))]
    public async Task<ActionResult<Exercise>> GetExerciseByIdAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        var serviceResult = await exerciseService.GetExerciseByIdAsync(exerciseId);
        return HandleExerciseServiceResult(serviceResult);
    }

    [HttpGet("user-exercise/{exerciseId}")]
    [ActionName(nameof(GetCurrentUserExerciseByIdAsync))]
    public async Task<ActionResult<Exercise>> GetCurrentUserExerciseByIdAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.GetUserExerciseByIdAsync(userId, exerciseId);
        return HandleServiceResult(serviceResult, "User exercise not found.");
    }

    [HttpGet("exercise/by-name/{name}")]
    public async Task<ActionResult<Exercise>> GetExerciseByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        var serviceResult = await exerciseService.GetExerciseByNameAsync(name);
        return HandleExerciseServiceResult(serviceResult);
    }

    [HttpGet("user-exercise/by-name/{name}")]
    public async Task<ActionResult<Exercise>> GetCurrentUserExerciseByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.GetUserExerciseByNameAsync(userId, name);
        return HandleServiceResult(serviceResult, "User exercise not found.");
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<ActionResult<Exercise>> AddExerciseAsync(Exercise exercise)
    {
        if (exercise is null)
            return ExerciseIsNull();

        if (exercise.Id != 0)
            return InvalidExerciseIDWhileAdding();

        var serviceResult = await exerciseService.AddExerciseAsync(exercise);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        exercise = serviceResult.Model!;

        return CreatedAtAction(nameof(GetExerciseByIdAsync), new { id = exercise.Id }, exercise);
    }

    [HttpPost("user-exercise")]
    public async Task<ActionResult<Exercise>> AddCurrentUserExerciseAsync(Exercise exercise)
    {
        if (exercise is null)
            return ExerciseIsNull();

        if (exercise.Id != 0)
            return InvalidExerciseIDWhileAdding();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.AddUserExerciseAsync(userId, exercise);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        exercise = serviceResult.Model!;

        return CreatedAtAction(nameof(GetCurrentUserExerciseByIdAsync), new { id = exercise.Id }, exercise);
    }

    [HttpPut("{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateExerciseAsync(long exerciseId, Exercise exercise)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (exercise is null)
            return ExerciseIsNull();

        if (exerciseId != exercise.Id)
            return ExerciseIDsNotMatch();

        var serviceResult = await exerciseService.UpdateExerciseAsync(exercise);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("user-exercise/{exerciseId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseAsync(long exerciseId, Exercise exercise)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (exercise is null)
            return ExerciseIsNull();

        if (exerciseId != exercise.Id)
            return ExerciseIDsNotMatch();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.UpdateUserExerciseAsync(userId, exercise);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteExerciseAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        var serviceResult = await exerciseService.DeleteExerciseAsync(exerciseId);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("user-exercise/{exerciseId}")]
    public async Task<IActionResult> DeleteExerciseFromCurrentUserAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.DeleteExerciseFromUserAsync(userId, exerciseId);
        return HandleServiceResult(serviceResult);
    }

    [HttpGet("exercise-exists/{exerciseId}")]
    public async Task<ActionResult<bool>> ExerciseExistsAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        try
        {
            return await exerciseService.ExerciseExistsAsync(exerciseId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("user-exercise-exists/{exerciseId}")]
    public async Task<ActionResult<bool>> CurrentUserExerciseExistsAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            return await exerciseService.UserExerciseExistsAsync(userId, exerciseId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("exercise-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> ExerciseExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        try
        {
            return await exerciseService.ExerciseExistsByNameAsync(name);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("user-exercise-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> CurrentUserExerciseExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            return await exerciseService.UserExerciseExistsByNameAsync(userId, name);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
