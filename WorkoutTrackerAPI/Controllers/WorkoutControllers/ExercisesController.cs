using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.ExerciseDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services.ExerciseServices;
using WorkoutTrackerAPI.Services.FileServices;

namespace ExerciseTrackerAPI.Controllers.ExerciseControllers;

public class ExercisesController : BaseWorkoutController<ExerciseDTO, ExerciseDTO>
{
    readonly IExerciseService exerciseService;
    readonly IFileService fileService;
    readonly IHttpContextAccessor httpContextAccessor;
    public ExercisesController(IExerciseService exerciseService, IFileService fileService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(mapper)
    {
        this.fileService = fileService;
        this.exerciseService = exerciseService;
        this.httpContextAccessor = httpContextAccessor;
    }


    const string exerciseNotFoundStr = "Exercise not found.";
    ActionResult<ExerciseDTO> HandleExerciseDTOServiceResult(ServiceResult<Exercise> serviceResult)
        => HandleDTOServiceResult<Exercise, ExerciseDTO>(serviceResult, exerciseNotFoundStr);
    ActionResult<ExerciseDetailsDTO> HandleExerciseDetailsDTOServiceResult(ServiceResult<Exercise> serviceResult)
        => HandleDTOServiceResult<Exercise, ExerciseDetailsDTO>(serviceResult, exerciseNotFoundStr);

    const string userExerciseNotFoundStr = "User exercise not found.";
    ActionResult<ExerciseDTO> HandleUserExerciseDTOServiceResult(ServiceResult<Exercise> serviceResult)
        => HandleDTOServiceResult<Exercise, ExerciseDTO>(serviceResult, userExerciseNotFoundStr);
    ActionResult<ExerciseDetailsDTO> HandleUserExerciseDetailsDTOServiceResult(ServiceResult<Exercise> serviceResult)
        => HandleDTOServiceResult<Exercise, ExerciseDetailsDTO>(serviceResult, userExerciseNotFoundStr);

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
        ExerciseType? type = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await exerciseService.GetInternalExercisesAsync(type);

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
        ExerciseType? type = null,
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
        var serviceResult = await exerciseService.GetUserExercisesAsync(userId, type);

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
        ExerciseType? type = null,
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
        var serviceResult = await exerciseService.GetAllExercisesAsync(userId, type);

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

    [HttpGet("internal-exercise/{exerciseId}/details")]
    [ActionName(nameof(GetInternalExerciseDetailsByIdAsync))]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetInternalExerciseDetailsByIdAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        var serviceResult = await exerciseService.GetInternalExerciseByIdAsync(exerciseId);
        return HandleExerciseDetailsDTOServiceResult(serviceResult);
    }


    [HttpGet("user-exercise/{exerciseId}")]
    [ActionName(nameof(GetCurrentUserExerciseByIdAsync))]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentUserExerciseByIdAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.GetUserExerciseByIdAsync(userId, exerciseId);
        return HandleUserExerciseDTOServiceResult(serviceResult);
    }

    [HttpGet("user-exercise/{exerciseId}/details")]
    [ActionName(nameof(GetCurrentUserExerciseDetailsByIdAsync))]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetCurrentUserExerciseDetailsByIdAsync(long exerciseId)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.GetUserExerciseByIdAsync(userId, exerciseId);
        return HandleUserExerciseDetailsDTOServiceResult(serviceResult);
    }


    [HttpGet("internal-exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetInternalExerciseByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        var serviceResult = await exerciseService.GetInternalExerciseByNameAsync(name);
        return HandleExerciseDTOServiceResult(serviceResult);
    }

    [HttpGet("internal-exercise/by-name/{name}/details")]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetInternalExerciseDetailsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        var serviceResult = await exerciseService.GetInternalExerciseByNameAsync(name);
        return HandleExerciseDetailsDTOServiceResult(serviceResult);
    }


    [HttpGet("user-exercise/by-name/{name}")]
    public async Task<ActionResult<ExerciseDTO>> GetCurrentUserExerciseByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.GetUserExerciseByNameAsync(userId, name);
        return HandleUserExerciseDTOServiceResult(serviceResult);
    }

    [HttpGet("user-exercise/by-name/{name}/details")]
    public async Task<ActionResult<ExerciseDetailsDTO>> GetCurrentUserExerciseDetailsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.GetUserExerciseByNameAsync(userId, name);
        return HandleUserExerciseDetailsDTOServiceResult(serviceResult);
    }

    readonly string exercisePhotosDirectory = Path.Combine("photos", "exercises");
    const int maxExerciseImageSizeInMB = 3;

    [HttpPost("internal-exercise")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddInternalExerciseAsync([FromForm] ExerciseCreationDTO exerciseCreationDTO)
    {
        if (exerciseCreationDTO is null)
            return ExerciseIsNull();

        try
        {
            string? image = await fileService.GetImage(exerciseCreationDTO.ImageFile, exercisePhotosDirectory, maxExerciseImageSizeInMB);
            var exercise = mapper.Map<Exercise>(exerciseCreationDTO);
            exercise.Image = image;

            var serviceResult = await exerciseService.AddInternalExerciseAsync(exercise);

            if (!serviceResult.Success)
                return BadRequest(serviceResult.ErrorMessage);

            exercise = serviceResult.Model!;

            return CreatedAtAction(nameof(GetInternalExerciseByIdAsync), new { exerciseId = exercise.Id }, exercise);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost("user-exercise")]
    public async Task<IActionResult> AddCurrentUserExerciseAsync([FromForm] ExerciseCreationDTO exerciseCreationDTO)
    {
        if (exerciseCreationDTO is null)
            return ExerciseIsNull();

        try
        {
            string? image = await fileService.GetImage(exerciseCreationDTO.ImageFile, exercisePhotosDirectory, maxExerciseImageSizeInMB);
            var exercise = mapper.Map<Exercise>(exerciseCreationDTO);
            exercise.Image = image;

            string userId = httpContextAccessor.GetUserId()!;
            var serviceResult = await exerciseService.AddUserExerciseAsync(userId, exercise);

            if (!serviceResult.Success)
                return BadRequest(serviceResult.ErrorMessage);

            exercise = serviceResult.Model!;

            return CreatedAtAction(nameof(GetCurrentUserExerciseByIdAsync), new { exerciseId = exercise.Id }, exercise);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    [HttpPut("internal-exercise/{exerciseId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseAsync(long exerciseId, [FromForm] ExerciseUpdateDTO exerciseUpdateDTO)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (exerciseUpdateDTO is null)
            return ExerciseIsNull();

        if (exerciseId != exerciseUpdateDTO.Id)
            return ExerciseIDsNotMatch();

        try
        {
            string? image = await fileService.GetImage(exerciseUpdateDTO.ImageFile, exercisePhotosDirectory, maxExerciseImageSizeInMB);
            var exercise = mapper.Map<Exercise>(exerciseUpdateDTO);
            exercise.Image = image ?? exerciseUpdateDTO.Image;

            var serviceResult = await exerciseService.UpdateInternalExerciseAsync(exercise);

            if (serviceResult.Success && exerciseUpdateDTO.ImageFile != null && exerciseUpdateDTO.Image is string oldImage)
            {
                fileService.DeleteFile(oldImage);
            }

            return HandleServiceResult(serviceResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("user-exercise/{exerciseId}")]
    public async Task<IActionResult> UpdateCurrentUserExerciseAsync(long exerciseId, [FromForm] ExerciseUpdateDTO exerciseUpdateDTO)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (exerciseUpdateDTO is null)
            return ExerciseIsNull();

        if (exerciseId != exerciseUpdateDTO.Id)
            return ExerciseIDsNotMatch();

        try
        {
            string? image = await fileService.GetImage(exerciseUpdateDTO.ImageFile, exercisePhotosDirectory, maxExerciseImageSizeInMB);
            var exercise = mapper.Map<Exercise>(exerciseUpdateDTO);
            exercise.Image = image ?? exerciseUpdateDTO.Image;

            string userId = httpContextAccessor.GetUserId()!;
            var serviceResult = await exerciseService.UpdateUserExerciseAsync(userId, exercise);

            if (serviceResult.Success && exerciseUpdateDTO.ImageFile != null && exerciseUpdateDTO.Image is string oldImage)
            {
                fileService.DeleteFile(oldImage);
            }

            return HandleServiceResult(serviceResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    [HttpPut("internal-exercise/{exerciseId}/muscles")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (muscleIds is null)
            return EntryIsNull("Muscle's Ids");

        var serviceResult = await exerciseService.UpdateInternalExerciseMusclesAsync(exerciseId, muscleIds);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("user-exercise/{exerciseId}/muscles")]
    public async Task<IActionResult> UpdateCurrentUserExerciseMusclesAsync(long exerciseId, IEnumerable<long> muscleIds)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (muscleIds is null)
            return EntryIsNull("Muscle's Ids");

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.UpdateUserExerciseMusclesAsync(userId, exerciseId, muscleIds);
        return HandleServiceResult(serviceResult);
    }


    [HttpPut("internal-exercise/{exerciseId}/equipments")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentsIds)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (equipmentsIds is null)
            return EntryIsNull("Equipment's Ids");

        var serviceResult = await exerciseService.UpdateInternalExerciseEquipmentsAsync(exerciseId, equipmentsIds);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("user-exercise/{exerciseId}/equipments")]
    public async Task<IActionResult> UpdateCurrentUserExerciseEquipmentsAsync(long exerciseId, IEnumerable<long> equipmentsIds)
    {
        if (exerciseId < 1)
            return InvalidExerciseID();

        if (equipmentsIds is null)
            return EntryIsNull("Equipment's Ids");

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await exerciseService.UpdateUserExerciseEquipmentsAsync(userId, exerciseId, equipmentsIds);
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
