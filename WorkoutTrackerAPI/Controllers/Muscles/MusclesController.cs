using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.API.Results;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.API.Models.Requests;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;

namespace WorkoutTracker.API.Controllers.WorkoutControllers;

public class MusclesController : BaseWorkoutController<MuscleDTO, MuscleDTO>
{
    readonly IMuscleService muscleService;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly IFileService fileService;
    public MusclesController (
        IFileService fileService, 
        IMuscleService muscleService, 
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.muscleService = muscleService;
        this.fileService = fileService;
        this.httpContextAccessor = httpContextAccessor;
    }

    readonly string musclePhotosDirectory = Path.Combine("photos", "muscles");
    const int maxMuscleImageSizeInMB = 3;


    [HttpGet]
    public async Task<ActionResult<ApiResult<MuscleDTO>>> GetMusclesAsync(
        long? parentMuscleId = null,
        bool? isMeasurable = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (parentMuscleId.HasValue && !IsValidID(parentMuscleId.Value))
            return InvalidMuscleID();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var muscles = await muscleService.GetMusclesAsync(parentMuscleId, isMeasurable);

        var muscleDTOs = muscles.ToList().Select(mapper.Map<MuscleDTO>);
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
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var parentMuscles = await muscleService.GetParentMusclesAsync();

        var parentMuscleDTOs = parentMuscles.ToList().Select(mapper.Map<MuscleDTO>);
        return await ApiResult<MuscleDTO>.CreateAsync(
            parentMuscleDTOs.AsQueryable(),
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
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var childMuscles = await muscleService.GetChildMusclesAsync();

        var childMuscleDTOs = childMuscles.ToList().Select(mapper.Map<MuscleDTO>);
        return await ApiResult<MuscleDTO>.CreateAsync(
            childMuscleDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }


    [HttpGet("{muscleId}/muscles")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetExercisesByMuscleIdAsync(
        long muscleId,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var muscle = await muscleService.GetMuscleByIdAsync(muscleId, userId, true);

        if (muscle is null)
            return EntryNotFound(nameof(Muscle));

        if (muscle.Exercises is not IEnumerable<Exercise> exercises)
            return EntryNotFound("Exercises");

        var exerciseDTOs = exercises.ToList().Select(mapper.Map<ExerciseDTO>);
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
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var muscle = await muscleService.GetMuscleByIdAsync(muscleId, userId);
        return ToMuscleDTO(muscle);
    }

    [HttpGet("{muscleId}/details")]
    [ActionName(nameof(GetMuscleDetailsByIdAsync))]
    public async Task<ActionResult<MuscleDetailsDTO>> GetMuscleDetailsByIdAsync(long muscleId)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleWithDetails = await muscleService.GetMuscleByIdAsync(muscleId, userId, true);
        return ToMuscleDetailsDTO(muscleWithDetails);
    }


    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<MuscleDTO>> GetMuscleByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return MuscleNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var muscle = await muscleService.GetMuscleByNameAsync(name, userId);
        return ToMuscleDTO(muscle);
    }

    [HttpGet("by-name/{name}/details")]
    public async Task<ActionResult<MuscleDetailsDTO>> GetMuscleDetailsByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return MuscleNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleWithDetails = await muscleService.GetMuscleByNameAsync(name, userId, true);
        return ToMuscleDetailsDTO(muscleWithDetails);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddMuscleAsync([FromForm] UploadWithPhoto<MuscleCreationDTO> muscleCreationDTOWithPhoto)
    {
        var (muscleCreationDTO, imageFile) = (muscleCreationDTOWithPhoto.Model, muscleCreationDTOWithPhoto.Photo);

        if (muscleCreationDTO is null)
            return MuscleIsNull();

        string? image = await fileService.GetImage(imageFile, musclePhotosDirectory, maxMuscleImageSizeInMB, false);
        var muscle = mapper.Map<Muscle>(muscleCreationDTO);
        muscle.Image = image;

        muscle = await muscleService.AddMuscleAsync(muscle);
        var muscleDTO = mapper.Map<MuscleDTO>(muscle);

        return CreatedAtAction(nameof(GetMuscleByIdAsync), new { muscleId = muscle.Id }, muscleDTO);
    }

    [HttpPut("{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateMuscleAsync(long muscleId, [FromForm] UploadWithPhoto<MuscleUpdateDTO> muscleUpdateDTOWithPhoto)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        var (muscleUpdateDTO, imageFile) = (muscleUpdateDTOWithPhoto.Model, muscleUpdateDTOWithPhoto.Photo);

        if (muscleUpdateDTO is null)
            return MuscleIsNull();

        if (!AreIdsEqual(muscleId, muscleUpdateDTO.Id))
            return EntryIDsNotMatch(nameof(Muscle));

        string? image = await fileService.GetImage(imageFile, musclePhotosDirectory, maxMuscleImageSizeInMB, false);
        var muscle = mapper.Map<Muscle>(muscleUpdateDTO);
        muscle.Image = image ?? muscleUpdateDTO.Image;

        await muscleService.UpdateMuscleAsync(muscle);

        if (imageFile != null && muscleUpdateDTO.Image is string oldImage)
            fileService.DeleteFile(oldImage);

        return Ok();
    }

    [HttpPut("{muscleId}/children")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        if (muscleChildIDs is null)
            return EntryIsNull("Muscle child's Ids");

        await muscleService.UpdateMuscleChildrenAsync(muscleId, muscleChildIDs);
        return Ok();
    }

    [HttpDelete("{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteMuscleAsync(long muscleId)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        await muscleService.DeleteMuscleAsync(muscleId);
        return Ok();
    }


    const string muscleNotFoundStr = "Muscle not found.";
    ActionResult<MuscleDTO> ToMuscleDTO(Muscle? muscle)
        => ToDTO<Muscle, MuscleDTO>(muscle, muscleNotFoundStr);
    ActionResult<MuscleDetailsDTO> ToMuscleDetailsDTO(Muscle? muscle)
        => ToDTO<Muscle, MuscleDetailsDTO>(muscle, muscleNotFoundStr);

    ActionResult InvalidMuscleID()
        => InvalidEntryID(nameof(Muscle));
    ActionResult MuscleNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Muscle));
    ActionResult MuscleIsNull()
        => EntryIsNull(nameof(Muscle));
}
