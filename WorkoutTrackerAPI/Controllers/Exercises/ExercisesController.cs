using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.API.Models.Requests;

namespace ExerciseTrackerAPI.Controllers.ExerciseControllers;

public class ExercisesController : BaseWorkoutController<ExerciseDTO, ExerciseDTO>
{
    readonly IExerciseService exerciseService;
    readonly IFileService fileService;
    readonly IHttpContextAccessor httpContextAccessor;
    public ExercisesController (
        IExerciseService exerciseService, 
        IFileService fileService, 
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.fileService = fileService;
        this.exerciseService = exerciseService;
        this.httpContextAccessor = httpContextAccessor;
    }

    readonly string exercisePhotosDirectory = Path.Combine("photos", "exercises");
    const int maxExerciseImageSizeInMB = 3;


    #region Internal Exercises

    [HttpGet("internal-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetInternalExercisesAsync(
        ExerciseType? type = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var internalExercises = await exerciseService.GetInternalExercisesAsync(type);

        var exerciseDTOs = internalExercises.ToList().Select(mapper.Map<ExerciseDTO>);
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
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var internalExercise = await exerciseService.GetInternalExerciseByIdAsync(userId, exerciseId);
        return ToExerciseDTO(internalExercise);
    }

    [HttpGet("internal-exercise/{exerciseId}/details")]
    [ActionName(nameof(GetInternalExerciseDetailsByIdAsync))]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetInternalExerciseDetailsByIdAsync(long exerciseId)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var internalExerciseWithDetails = await exerciseService.GetInternalExerciseByIdAsync(userId, exerciseId, true);
        return ToExerciseDetailsDTO(internalExerciseWithDetails);
    }

    [HttpGet("internal-exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetInternalExerciseByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var internalExercise = await exerciseService.GetInternalExerciseByNameAsync(userId, name);
        return ToExerciseDTO(internalExercise);
    }

    [HttpGet("internal-exercise/by-name/{name}/details")]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetInternalExerciseDetailsByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var internalExerciseWithDetails = await exerciseService.GetInternalExerciseByNameAsync(userId, name, true);
        return ToExerciseDetailsDTO(internalExerciseWithDetails);
    }

    [HttpPost("internal-exercise")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddInternalExerciseAsync([FromForm] UploadWithPhoto<ExerciseCreationDTO> exerciseCreationDTOWithPhoto)
    {
        var (exerciseCreationDTO, imageFile) = (exerciseCreationDTOWithPhoto.Model, exerciseCreationDTOWithPhoto.Photo);

        if (exerciseCreationDTO is null)
            return ExerciseIsNull();

        string? image = await fileService.GetImage(imageFile, exercisePhotosDirectory, maxExerciseImageSizeInMB, false);
        var exercise = mapper.Map<Exercise>(exerciseCreationDTO);
        exercise.Image = image;

        exercise = await exerciseService.AddInternalExerciseAsync(exercise);
        var exerciseDTO = mapper.Map<ExerciseDTO>(exercise);

        return CreatedAtAction(nameof(GetInternalExerciseByIdAsync), new { exerciseId = exercise.Id }, exerciseDTO);
    }


    [HttpPut("internal-exercise/{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseAsync(long exerciseId, [FromForm] UploadWithPhoto<ExerciseUpdateDTO> exerciseUpdateDTOWithPhoto)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        var (exerciseUpdateDTO, imageFile) = (exerciseUpdateDTOWithPhoto.Model, exerciseUpdateDTOWithPhoto.Photo);

        if (exerciseUpdateDTO is null)
            return ExerciseIsNull();

        if (!AreIdsEqual(exerciseId, exerciseUpdateDTO.Id))
            return ExerciseIDsNotMatch();

        string? image = await fileService.GetImage(imageFile, exercisePhotosDirectory, maxExerciseImageSizeInMB, false);
        var exercise = mapper.Map<Exercise>(exerciseUpdateDTO);
        exercise.Image = image ?? exerciseUpdateDTO.Image;

        await exerciseService.UpdateInternalExerciseAsync(exercise);

        if (imageFile != null && exerciseUpdateDTO.Image is string oldImage)
            fileService.DeleteFile(oldImage);

        return Ok();
    }

    [HttpPut("internal-exercise/{exerciseId}/muscles")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (muscleIds is null)
            return EntryIsNull("Muscle's Ids");

        await exerciseService.UpdateInternalExerciseMusclesAsync(exerciseId, muscleIds);
        return Ok();
    }

    [HttpPut("internal-exercise/{exerciseId}/equipments")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentsIds)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (equipmentsIds is null)
            return EntryIsNull("Equipment's Ids");

        await exerciseService.UpdateInternalExerciseEquipmentsAsync(exerciseId, equipmentsIds);
        return Ok();
    }

    [HttpDelete("internal-exercise/{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteInternalExerciseAsync(long exerciseId)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        await exerciseService.DeleteInternalExerciseAsync(exerciseId);
        return Ok();
    }

    #endregion

    #region User Exercises

    [HttpGet("user-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetCurrentUserExercisesAsync(
        ExerciseType? type = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var usedExercises = await exerciseService.GetUserExercisesAsync(userId, type);

        var exerciseDTOs = usedExercises.ToList().Select(mapper.Map<ExerciseDTO>);
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

    [HttpGet("user-exercise/{exerciseId}")]
    [ActionName(nameof(GetCurrentUserExerciseByIdAsync))]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentUserExerciseByIdAsync(long exerciseId)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var userExercise = await exerciseService.GetUserExerciseByIdAsync(userId, exerciseId);
        return ToExerciseDTO(userExercise);
    }

    [HttpGet("user-exercise/{exerciseId}/details")]
    [ActionName(nameof(GetCurrentUserExerciseDetailsByIdAsync))]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetCurrentUserExerciseDetailsByIdAsync(long exerciseId)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var userExerciseWithDetails = await exerciseService.GetUserExerciseByIdAsync(userId, exerciseId, true);
        return ToExerciseDetailsDTO(userExerciseWithDetails);
    }

    [HttpGet("user-exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentUserExerciseByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var userExercise = await exerciseService.GetUserExerciseByNameAsync(userId, name);
        return ToExerciseDTO(userExercise);
    }

    [HttpGet("user-exercise/by-name/{name}/details")]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetCurrentUserExerciseDetailsByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var userExerciseWithDetails = await exerciseService.GetUserExerciseByNameAsync(userId, name, true);
        return ToExerciseDetailsDTO(userExerciseWithDetails);
    }

    [HttpPost("user-exercise")]
    public async Task<IActionResult> AddCurrentUserExerciseAsync([FromForm] UploadWithPhoto<ExerciseCreationDTO> exerciseCreationDTOWithPhoto)
    {
        var (exerciseCreationDTO, imageFile) = (exerciseCreationDTOWithPhoto.Model, exerciseCreationDTOWithPhoto.Photo);

        if (exerciseCreationDTO is null)
            return ExerciseIsNull();

        string? image = await fileService.GetImage(imageFile, exercisePhotosDirectory, maxExerciseImageSizeInMB);
        var exercise = mapper.Map<Exercise>(exerciseCreationDTO);
        exercise.Image = image;

        string userId = httpContextAccessor.GetUserId()!;
        exercise = await exerciseService.AddUserExerciseAsync(userId, exercise);
        var exerciseDTO = mapper.Map<ExerciseDTO>(exercise);

        return CreatedAtAction(nameof(GetCurrentUserExerciseByIdAsync), new { exerciseId = exercise.Id }, exerciseDTO);
    }

    [HttpPut("user-exercise/{exerciseId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseAsync(long exerciseId, [FromForm] UploadWithPhoto<ExerciseUpdateDTO> exerciseUpdateDTOWithPhoto)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        var (exerciseUpdateDTO, imageFile) = (exerciseUpdateDTOWithPhoto.Model, exerciseUpdateDTOWithPhoto.Photo);

        if (exerciseUpdateDTO is null)
            return ExerciseIsNull();

        if (!AreIdsEqual(exerciseId, exerciseUpdateDTO.Id))
            return ExerciseIDsNotMatch();

        string? image = await fileService.GetImage(imageFile, exercisePhotosDirectory, maxExerciseImageSizeInMB);
        var exercise = mapper.Map<Exercise>(exerciseUpdateDTO);
        exercise.Image = image ?? exerciseUpdateDTO.Image;

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.UpdateUserExerciseAsync(userId, exercise);

        if (imageFile != null && exerciseUpdateDTO.Image is string oldImage)
            fileService.DeleteFile(oldImage);

        return Ok();
    }

    [HttpPut("user-exercise/{exerciseId}/muscles")]
    public async Task<IActionResult> UpdateCurrentUserExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (muscleIds is null)
            return EntryIsNull("Muscle's Ids");

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.UpdateUserExerciseMusclesAsync(userId, exerciseId, muscleIds);
        return Ok();
    }

    [HttpPut("user-exercise/{exerciseId}/equipments")]
    public async Task<IActionResult> UpdateCurrentUserExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentsIds)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        if (equipmentsIds is null)
            return EntryIsNull("Equipment's Ids");

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.UpdateUserExerciseEquipmentsAsync(userId, exerciseId, equipmentsIds);
        return Ok();
    }

    [HttpDelete("user-exercise/{exerciseId}")]
    public async Task<IActionResult> DeleteExerciseFromCurrentUserAsync(long exerciseId)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        await exerciseService.DeleteExerciseFromUserAsync(userId, exerciseId);
        return Ok();
    }

    #endregion

    #region All Exercises

    [HttpGet("all-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetAllExercisesAsync(
        ExerciseType? type = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var allExercises = await exerciseService.GetAllExercisesAsync(userId, type);

        var exerciseDTOs = allExercises.ToList().Select(mapper.Map<ExerciseDTO>);
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

    [HttpGet("used-exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetUsedExercisesAsync(
        ExerciseType? type = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var usedExercises = await exerciseService.GetUsedExercisesAsync(userId, type);

        var exerciseDTOs = usedExercises.ToList().Select(mapper.Map<ExerciseDTO>);
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

    [HttpGet("exercise/{exerciseId}")]
    [ActionName(nameof(GetCurrentExerciseByIdAsync))]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentExerciseByIdAsync(long exerciseId)
    {
        if (!IsValidID(exerciseId))
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var exercise = await exerciseService.GetExerciseByIdAsync(userId, exerciseId);
        return ToExerciseDTO(exercise);
    }

    [HttpGet("exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetExerciseByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var exercise = await exerciseService.GetExerciseByNameAsync(userId, name);
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
