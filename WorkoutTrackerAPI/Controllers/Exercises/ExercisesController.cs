using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Application.Common.Models;

namespace ExerciseTrackerAPI.Controllers.ExerciseControllers;

public class ExercisesController : BaseWorkoutController<ExerciseDTO, ExerciseDTO>
{
    readonly IExerciseService exerciseService;
    readonly IHttpContextAccessor httpContextAccessor;
    public ExercisesController (
        IExerciseService exerciseService, 
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.exerciseService = exerciseService;
        this.httpContextAccessor = httpContextAccessor;
    }

    #region Internal Exercises

    [HttpGet("internal-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetInternalExercisesAsync(
        CancellationToken cancellationToken,
        [FromQuery] ExerciseType? type = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var internalExercises = await exerciseService.GetInternalExercisesAsync(type, filterQuery, cancellationToken);

        var exerciseDTOs = internalExercises.Select(mapper.Map<ExerciseDTO>);
        return await ApiResult<ExerciseDTO>.CreateAsync(
            exerciseDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder
        );
    }

    [HttpGet("internal-exercise/{exerciseId}")]
    [ActionName(nameof(GetInternalExerciseByIdAsync))]
    public async Task<ActionResult<ExerciseDTO>> GetInternalExerciseByIdAsync(long exerciseId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var internalExercise = await exerciseService.GetInternalExerciseByIdAsync(userId, exerciseId, cancellationToken);
        return ToExerciseDTO(internalExercise);
    }

    [HttpGet("internal-exercise/{exerciseId}/details")]
    [ActionName(nameof(GetInternalExerciseDetailsByIdAsync))]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetInternalExerciseDetailsByIdAsync(long exerciseId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var internalExerciseWithDetails = await exerciseService.GetInternalExerciseByIdWithDetailsAsync(userId, exerciseId, cancellationToken);
        return ToExerciseDetailsDTO(internalExerciseWithDetails);
    }

    [HttpGet("internal-exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetInternalExerciseByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var internalExercise = await exerciseService.GetInternalExerciseByNameAsync(userId, name, cancellationToken);
        return ToExerciseDTO(internalExercise);
    }

    [HttpGet("internal-exercise/by-name/{name}/details")]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetInternalExerciseDetailsByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var internalExerciseWithDetails = await exerciseService.GetInternalExerciseByNameWithDetailsAsync(userId, name, cancellationToken);
        return ToExerciseDetailsDTO(internalExerciseWithDetails);
    }

    [HttpPost("internal-exercise")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddInternalExerciseAsync([FromBody] ExerciseCreationDTO exerciseCreationDTO, CancellationToken cancellationToken)
    {
        if (exerciseCreationDTO is null)
            return ExerciseIsNull();

        var exercise = mapper.Map<Exercise>(exerciseCreationDTO);
        exercise = await exerciseService.AddInternalExerciseAsync(exercise, cancellationToken);

        var exerciseDTO = mapper.Map<ExerciseDTO>(exercise);
        return CreatedAtAction(nameof(GetInternalExerciseByIdAsync), new { exerciseId = exercise.Id }, exerciseDTO);
    }


    [HttpPut("internal-exercise/{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseAsync(long exerciseId, [FromBody] ExerciseUpdateDTO exerciseUpdateDTO, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (exerciseUpdateDTO is null)
            return ExerciseIsNull();

        if (!AreIdsEqual(exerciseId, exerciseUpdateDTO.Id))
            return ExerciseIDsNotMatch();

        var exercise = mapper.Map<Exercise>(exerciseUpdateDTO);
        await exerciseService.UpdateInternalExerciseAsync(exercise, cancellationToken);

        return Ok();
    }

    [HttpPut("internal-exercise/{exerciseId}/muscles")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (muscleIds is null)
            return EntryIsNull("Muscle's Ids");

        await exerciseService.UpdateInternalExerciseMusclesAsync(exerciseId, muscleIds, cancellationToken);
        return Ok();
    }

    [HttpPut("internal-exercise/{exerciseId}/equipments")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentsIds, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (equipmentsIds is null)
            return EntryIsNull("Equipment's Ids");

        await exerciseService.UpdateInternalExerciseEquipmentsAsync(exerciseId, equipmentsIds, cancellationToken);
        return Ok();
    }

    [HttpPut("internal-exercise/{exerciseId}/aliases")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseAliasesAsync(long exerciseId, string[] aliasesStr, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (aliasesStr is null)
            return EntryIsNull("aliases");

        await exerciseService.UpdateInternalExerciseAliasesAsync(exerciseId, aliasesStr, cancellationToken);
        return Ok();
    }

    [HttpDelete("internal-exercise/{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteInternalExerciseAsync(long exerciseId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        await exerciseService.DeleteInternalExerciseAsync(exerciseId, cancellationToken);
        return Ok();
    }

    [HttpPut("internal-exercise-photo/{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExercisePhotoAsync(long exerciseId, [FromForm] FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        await exerciseService.UpdateInternalExercisePhotoAsync(exerciseId, fileUpload, cancellationToken);
        return Ok();
    }

    [HttpDelete("internal-exercise-photo/{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteInternalExercisePhotoAsync(long exerciseId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        await exerciseService.DeleteInternalExercisePhotoAsync(exerciseId, cancellationToken);
        return Ok();
    }

    #endregion

    #region User Exercises

    [HttpGet("user-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetCurrentUserExercisesAsync(CancellationToken cancellationToken,
        [FromQuery] ExerciseType? type = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var usedExercises = await exerciseService.GetUserExercisesAsync(userId, type, filterQuery, cancellationToken);

        var exerciseDTOs = usedExercises.Select(mapper.Map<ExerciseDTO>);
        return await ApiResult<ExerciseDTO>.CreateAsync(
            exerciseDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder
        );
    }

    [HttpGet("user-exercise/{exerciseId}")]
    [ActionName(nameof(GetCurrentUserExerciseByIdAsync))]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentUserExerciseByIdAsync(long exerciseId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var userExercise = await exerciseService.GetUserExerciseByIdAsync(userId, exerciseId, cancellationToken);
        return ToExerciseDTO(userExercise);
    }

    [HttpGet("user-exercise/{exerciseId}/details")]
    [ActionName(nameof(GetCurrentUserExerciseDetailsByIdAsync))]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetCurrentUserExerciseDetailsByIdAsync(long exerciseId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var userExerciseWithDetails = await exerciseService.GetUserExerciseByIdWithDetailsAsync(userId, exerciseId, cancellationToken);
        return ToExerciseDetailsDTO(userExerciseWithDetails);
    }

    [HttpGet("user-exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentUserExerciseByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var userExercise = await exerciseService.GetUserExerciseByNameAsync(userId, name, cancellationToken);
        return ToExerciseDTO(userExercise);
    }

    [HttpGet("user-exercise/by-name/{name}/details")]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetCurrentUserExerciseDetailsByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var userExerciseWithDetails = await exerciseService.GetUserExerciseByNameWithDetailsAsync(userId, name, cancellationToken);
        return ToExerciseDetailsDTO(userExerciseWithDetails);
    }

    [HttpPost("user-exercise")]
    public async Task<IActionResult> AddCurrentUserExerciseAsync([FromBody] ExerciseCreationDTO exerciseCreationDTO, CancellationToken cancellationToken)
    {
        if (exerciseCreationDTO is null)
            return ExerciseIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var exercise = mapper.Map<Exercise>(exerciseCreationDTO);
        exercise = await exerciseService.AddUserExerciseAsync(userId, exercise, cancellationToken);

        var exerciseDTO = mapper.Map<ExerciseDTO>(exercise);
        return CreatedAtAction(nameof(GetCurrentUserExerciseByIdAsync), new { exerciseId = exercise.Id }, exerciseDTO);
    }

    [HttpPut("user-exercise/{exerciseId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseAsync(long exerciseId, [FromBody] ExerciseUpdateDTO exerciseUpdateDTO, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (exerciseUpdateDTO is null)
            return ExerciseIsNull();

        if (!AreIdsEqual(exerciseId, exerciseUpdateDTO.Id))
            return ExerciseIDsNotMatch();

        string userId = httpContextAccessor.GetUserId()!;
        var exercise = mapper.Map<Exercise>(exerciseUpdateDTO);
        await exerciseService.UpdateUserExerciseAsync(userId, exercise, cancellationToken);

        return Ok();
    }

    [HttpPut("user-exercise/{exerciseId}/muscles")]
    public async Task<IActionResult> UpdateCurrentUserExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (muscleIds is null)
            return EntryIsNull("Muscle's Ids");

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.UpdateUserExerciseMusclesAsync(userId, exerciseId, muscleIds, cancellationToken);
        return Ok();
    }

    [HttpPut("user-exercise/{exerciseId}/equipments")]
    public async Task<IActionResult> UpdateCurrentUserExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentsIds, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (equipmentsIds is null)
            return EntryIsNull("Equipment's Ids");

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.UpdateUserExerciseEquipmentsAsync(userId, exerciseId, equipmentsIds, cancellationToken);
        return Ok();
    }

    [HttpPut("user-exercise/{exerciseId}/aliases")]
    public async Task<IActionResult> UpdateUserExerciseAliasesAsync(long exerciseId, string[] aliasesStr, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (aliasesStr is null)
            return EntryIsNull("aliases");

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.UpdateUserExerciseAliasesAsync(userId, exerciseId, aliasesStr, cancellationToken);
        return Ok();
    }

    [HttpDelete("user-exercise/{exerciseId}")]
    public async Task<IActionResult> DeleteExerciseFromCurrentUserAsync(long exerciseId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.DeleteExerciseFromUserAsync(userId, exerciseId, cancellationToken);
        return Ok();
    }

    [HttpPut("user-exercise-photo/{exerciseId}")]
    public async Task<IActionResult> UpdateUserExercisePhotoAsync(long exerciseId, [FromForm] FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.UpdateUserExercisePhotoAsync(userId, exerciseId, fileUpload, cancellationToken);
        return Ok();
    }

    [HttpDelete("user-exercise-photo/{exerciseId}")]
    public async Task<IActionResult> DeleteUserExercisePhotoAsync(long exerciseId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.DeleteUserExercisePhotoAsync(userId, exerciseId, cancellationToken);
        return Ok();
    }

    #endregion

    #region All Exercises

    [HttpGet("all-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetAllExercisesAsync(CancellationToken cancellationToken,
        [FromQuery] ExerciseType? type = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var allExercises = await exerciseService.GetAllExercisesAsync(userId, type, filterQuery, cancellationToken);

        var exerciseDTOs = allExercises.Select(mapper.Map<ExerciseDTO>);
        return await ApiResult<ExerciseDTO>.CreateAsync(
            exerciseDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder
        );
    }

    [HttpGet("used-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetUsedExercisesAsync(CancellationToken cancellationToken,
        [FromQuery] ExerciseType? type = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var usedExercises = await exerciseService.GetUsedExercisesAsync(userId, type, filterQuery, cancellationToken);

        var exerciseDTOs = usedExercises.Select(mapper.Map<ExerciseDTO>);
        return await ApiResult<ExerciseDTO>.CreateAsync(
            exerciseDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder
        );
    }

    [HttpGet("exercise/{exerciseId}")]
    [ActionName(nameof(GetCurrentExerciseByIdAsync))]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentExerciseByIdAsync(long exerciseId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var exercise = await exerciseService.GetExerciseByIdAsync(userId, exerciseId, cancellationToken);
        return ToExerciseDTO(exercise);
    }

    [HttpGet("exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetExerciseByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var exercise = await exerciseService.GetExerciseByNameAsync(userId, name, cancellationToken);
        return ToExerciseDTO(exercise);
    }

    #endregion


    const string exerciseNotFoundStr = "Exercise not found.";
    ActionResult<ExerciseDTO> ToExerciseDTO(Exercise? exercise)
        => ToDTO<Exercise, ExerciseDTO>(exercise, exerciseNotFoundStr);
    ActionResult<ExerciseDetailsDTO> ToExerciseDetailsDTO(Exercise? exercise)
        => ToDTO<Exercise, ExerciseDetailsDTO>(exercise, exerciseNotFoundStr);

    ActionResult InvalidExerciseID()
        => InvalidEntryID(nameof(Exercise));
    ActionResult ExerciseNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Exercise));
    ActionResult ExerciseIsNull()
        => EntryIsNull(nameof(Exercise));
    ActionResult ExerciseIDsNotMatch()
        => EntryIDsNotMatch(nameof(Exercise));
}
