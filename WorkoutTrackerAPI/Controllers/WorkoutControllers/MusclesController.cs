using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Services.MuscleServices;
using Microsoft.AspNetCore.Authorization;
using WorkoutTrackerAPI.Data.DTOs;
using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.MuscleDTOs;
using WorkoutTrackerAPI.Services.FileServices;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.ExerciseDTOs;
using WorkoutTrackerAPI.Services;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public class MusclesController : BaseWorkoutController<MuscleDTO, MuscleDTO>
{
    readonly IMuscleService muscleService;
    readonly IFileService fileService;
    public MusclesController(IFileService fileService, IMuscleService muscleService, IMapper mapper) : base(mapper)
    {
        this.muscleService = muscleService;
        this.fileService = fileService;
    }


    const string muscleNotFoundStr = "Muscle not found.";
    ActionResult<MuscleDTO> HandleMuscleDTOServiceResult(ServiceResult<Muscle> serviceResult)
        => HandleDTOServiceResult<Muscle, MuscleDTO>(serviceResult, muscleNotFoundStr);
    ActionResult<MuscleDetailsDTO> HandleMuscleDetailsDTOServiceResult(ServiceResult<Muscle> serviceResult)
        => HandleDTOServiceResult<Muscle, MuscleDetailsDTO>(serviceResult, muscleNotFoundStr);

    ActionResult InvalidMuscleID()
        => InvalidEntryID(nameof(Muscle));
    ActionResult MuscleNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Muscle));
    ActionResult MuscleIsNull()
        => EntryIsNull(nameof(Muscle));


    [HttpGet]
    public async Task<ActionResult<ApiResult<MuscleDTO>>> GetMusclesAsync(
        [FromQuery] long? parentMuscleId = null,
        [FromQuery] bool? isMeasurable = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (parentMuscleId.HasValue && parentMuscleId < 1)
            return InvalidMuscleID();

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await muscleService.GetMusclesAsync(parentMuscleId, isMeasurable);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Muscle> muscles)
            return EntryNotFound("Muscles");

        var muscleDTOs = muscles.AsEnumerable().Select(m => mapper.Map<MuscleDTO>(m));
        return await ApiResult<MuscleDTO>.CreateAsync(
            muscleDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("parent-muscles")]
    public async Task<ActionResult<ApiResult<MuscleDTO>>> GetParentMusclesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await muscleService.GetParentMusclesAsync();

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Muscle> muscles)
            return EntryNotFound("Muscles");

        var muscleDTOs = muscles.AsEnumerable().Select(m => mapper.Map<MuscleDTO>(m));
        return await ApiResult<MuscleDTO>.CreateAsync(
            muscleDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("child-muscles")]
    public async Task<ActionResult<ApiResult<MuscleDTO>>> GetChildMusclesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await muscleService.GetChildMusclesAsync();

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Muscle> muscles)
            return EntryNotFound("Muscles");

        var muscleDTOs = muscles.AsEnumerable().Select(m => mapper.Map<MuscleDTO>(m));
        return await ApiResult<MuscleDTO>.CreateAsync(
            muscleDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }


    [HttpGet("{muscleId}/exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetExercisesByMuscleIdAsync(
        long muscleId,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await muscleService.GetMuscleByIdAsync(muscleId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not Muscle muscle)
            return EntryNotFound("Muscle");

        if (muscle.Exercises is not IEnumerable<Exercise> exercises)
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

    [HttpGet("{muscleId}")]
    [ActionName(nameof(GetMuscleByIdAsync))]
    public async Task<ActionResult<MuscleDTO>> GetMuscleByIdAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        var serviceResult = await muscleService.GetMuscleByIdAsync(muscleId);
        return HandleMuscleDTOServiceResult(serviceResult);
    }

    [HttpGet("{muscleId}/details")]
    [ActionName(nameof(GetMuscleDetailsByIdAsync))]
    public async Task<ActionResult<MuscleDetailsDTO>> GetMuscleDetailsByIdAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        var serviceResult = await muscleService.GetMuscleByIdAsync(muscleId);
        return HandleMuscleDetailsDTOServiceResult(serviceResult);
    }


    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<MuscleDTO>> GetMuscleByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return MuscleNameIsNullOrEmpty();

        var serviceResult = await muscleService.GetMuscleByNameAsync(name);
        return HandleMuscleDTOServiceResult(serviceResult);
    }

    [HttpGet("by-name/{name}/details")]
    public async Task<ActionResult<MuscleDetailsDTO>> GetMuscleDetailsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return MuscleNameIsNullOrEmpty();

        var serviceResult = await muscleService.GetMuscleByNameAsync(name);
        return HandleMuscleDetailsDTOServiceResult(serviceResult);
    }

    readonly string musclePhotosDirectory = Path.Combine("photos", "muscles");
    const int maxMuscleImageSizeInMB = 3;

    [HttpPost]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddMuscleAsync([FromForm] MuscleCreationDTO muscleCreationDTO)
    {
        if (muscleCreationDTO is null)
            return MuscleIsNull();

        try
        {
            string? image = await fileService.GetImage(muscleCreationDTO.ImageFile, musclePhotosDirectory, maxMuscleImageSizeInMB);
            var muscle = mapper.Map<Muscle>(muscleCreationDTO);
            muscle.Image = image;

            var serviceResult = await muscleService.AddMuscleAsync(muscle);

            if (!serviceResult.Success)
                return BadRequest(serviceResult.ErrorMessage);

            muscle = serviceResult.Model!;

            return CreatedAtAction(nameof(GetMuscleByIdAsync), new { muscleId = muscle.Id }, muscle);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateMuscleAsync(long muscleId, [FromForm] MuscleUpdateDTO muscleUpdateDTO)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        if (muscleUpdateDTO is null)
            return MuscleIsNull();

        if (muscleId != muscleUpdateDTO.Id)
            return EntryIDsNotMatch(nameof(Muscle));

        try
        {
            string? image = await fileService.GetImage(muscleUpdateDTO.ImageFile, musclePhotosDirectory, maxMuscleImageSizeInMB);
            var muscle = mapper.Map<Muscle>(muscleUpdateDTO);
            muscle.Image = image ?? muscleUpdateDTO.Image;

            var serviceResult = await muscleService.UpdateMuscleAsync(muscle);

            if (serviceResult.Success && muscleUpdateDTO.ImageFile != null && muscleUpdateDTO.Image is string oldImage)
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

    [HttpPut("{muscleId}/children")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        if (muscleChildIDs is null)
            return EntryIsNull("Muscle child's Ids");

        var serviceResult = await muscleService.UpdateMuscleChildrenAsync(muscleId, muscleChildIDs);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteMuscleAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        var serviceResult = await muscleService.DeleteMuscleAsync(muscleId);
        return HandleServiceResult(serviceResult);
    }

    [HttpGet("muscle-exists/{muscleId}")]
    public async Task<ActionResult<bool>> MuscleExistsAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        try
        {
            return await muscleService.MuscleExistsAsync(muscleId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("muscle-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> MuscleExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return MuscleNameIsNullOrEmpty();

        try
        {
            return await muscleService.MuscleExistsByNameAsync(name);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
