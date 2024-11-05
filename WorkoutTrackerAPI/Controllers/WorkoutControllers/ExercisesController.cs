using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.ExerciseRecordServices;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Services.WorkoutServices;

namespace ExerciseTrackerAPI.Controllers.ExerciseControllers;

public class ExercisesController : BaseWorkoutController<ExerciseDTO, ExerciseDTO>
{
    readonly IExerciseService exerciseService;
    readonly IHttpContextAccessor httpContextAccessor;
    public ExercisesController(IExerciseService exerciseService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(mapper)
    {
        this.exerciseService = exerciseService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<ExerciseDTO> HandleExerciseDTOServiceResult(ServiceResult<Exercise> serviceResult)
        => HandleDTOServiceResult(serviceResult, "Exercise not found.");

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

    [HttpGet("internal-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetInternalExercisesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await exerciseService.GetInternalExercisesAsync();

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Exercise> exercises)
            return EntryNotFound("Exercises");

        var exerciseDTOs = exercises.AsEnumerable().Select(m => mapper.Map<ExerciseDTO>(m));
        return await ApiResult<ExerciseDTO>.CreateAsync(
            exerciseDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("user-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetCurrentUserExercisesAsync(
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

        if (serviceResult.Model is not IQueryable<Exercise> exercises)
            return EntryNotFound("Exercises");

        var exerciseDTOs = exercises.AsEnumerable().Select(m => mapper.Map<ExerciseDTO>(m));
        return await ApiResult<ExerciseDTO>.CreateAsync(
            exerciseDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("all-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetAllExercisesAsync(
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
        var serviceResult = await exerciseService.GetAllExercisesAsync(userId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Exercise> exercises)
            return EntryNotFound("Exercises");

        var exerciseDTOs = exercises.AsEnumerable().Select(m => mapper.Map<ExerciseDTO>(m));
        return await ApiResult<ExerciseDTO>.CreateAsync(
            exerciseDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("internal-exercise/{exerciseId}")]
    [ActionName(nameof(GetInternalExerciseByIdAsync))]
    public async Task<ActionResult<ExerciseDTO>> GetInternalExerciseByIdAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        var serviceResult = await exerciseService.GetInternalExerciseByIdAsync(exerciseId);
        return HandleExerciseDTOServiceResult(serviceResult);
    }

    [HttpGet("user-exercise/{exerciseId}")]
    [ActionName(nameof(GetCurrentUserExerciseByIdAsync))]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentUserExerciseByIdAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.GetUserExerciseByIdAsync(userId, exerciseId);
        return HandleDTOServiceResult(serviceResult, "User exercise not found.");
    }

    [HttpGet("internal-exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetInternalExerciseByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        var serviceResult = await exerciseService.GetInternalExerciseByNameAsync(name);
        return HandleExerciseDTOServiceResult(serviceResult);
    }

    [HttpGet("user-exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentUserExerciseByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.GetUserExerciseByNameAsync(userId, name);
        return HandleDTOServiceResult(serviceResult, "User exercise not found.");
    }

    [HttpPost("internal-exercise")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddInternalExerciseAsync(ExerciseDTO exerciseDTO)
    {
        if (exerciseDTO is null)
            return ExerciseIsNull();

        if (exerciseDTO.Id != 0)
            return InvalidExerciseIDWhileAdding();

        var exercise = mapper.Map<Exercise>(exerciseDTO);
        var serviceResult = await exerciseService.AddInternalExerciseAsync(exercise);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        exercise = serviceResult.Model!;

        return CreatedAtAction(nameof(GetInternalExerciseByIdAsync), new { exerciseId = exercise.Id }, exercise);
    }

    [HttpPost("user-exercise")]
    public async Task<IActionResult> AddCurrentUserExerciseAsync(ExerciseDTO exerciseDTO)
    {
        if (exerciseDTO is null)
            return ExerciseIsNull();

        if (exerciseDTO.Id != 0)
            return InvalidExerciseIDWhileAdding();

        string userId = httpContextAccessor.GetUserId()!;
        var exercise = mapper.Map<Exercise>(exerciseDTO);
        var serviceResult = await exerciseService.AddUserExerciseAsync(userId, exercise);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        exercise = serviceResult.Model!;

        return CreatedAtAction(nameof(GetCurrentUserExerciseByIdAsync), new { exerciseId = exercise.Id }, exercise);
    }

    [HttpPut("internal-exercise/{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseAsync(long exerciseId, ExerciseDTO exerciseDTO)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (exerciseDTO is null)
            return ExerciseIsNull();

        if (exerciseId != exerciseDTO.Id)
            return ExerciseIDsNotMatch();

        var exercise = mapper.Map<Exercise>(exerciseDTO);
        var serviceResult = await exerciseService.UpdateInternalExerciseAsync(exercise);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("user-exercise/{exerciseId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseAsync(long exerciseId, ExerciseDTO exerciseDTO)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (exerciseDTO is null)
            return ExerciseIsNull();

        if (exerciseId != exerciseDTO.Id)
            return ExerciseIDsNotMatch();

        string userId = httpContextAccessor.GetUserId()!;
        var exercise = mapper.Map<Exercise>(exerciseDTO);
        var serviceResult = await exerciseService.UpdateUserExerciseAsync(userId, exercise);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("internal-exercise/{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteInternalExerciseAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        var serviceResult = await exerciseService.DeleteInternalExerciseAsync(exerciseId);
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

    [HttpGet("internal-exercise-exists/{exerciseId}")]
    public async Task<ActionResult<bool>> InternalExerciseExistsAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        try
        {
            return await exerciseService.InternalExerciseExistsAsync(exerciseId);
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

    [HttpGet("internal-exercise-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> InternalExerciseExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        try
        {
            return await exerciseService.InternalExerciseExistsByNameAsync(name);
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
