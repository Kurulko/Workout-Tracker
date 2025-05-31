using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.API.Results;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.API.Controllers.WorkoutControllers;

public class MusclesController : BaseWorkoutController<MuscleDTO, MuscleDTO>
{
    readonly IMuscleService muscleService;
    readonly IHttpContextAccessor httpContextAccessor;
    public MusclesController (
        IMuscleService muscleService, 
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.muscleService = muscleService;
        this.httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResult<MuscleDTO>>> GetMusclesAsync(CancellationToken cancellationToken,
        [FromQuery] long? parentMuscleId = null,
        [FromQuery] bool? isMeasurable = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (parentMuscleId.HasValue && !IsValidID(parentMuscleId.Value))
            return InvalidMuscleID();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var muscles = await muscleService.GetMusclesAsync(parentMuscleId, isMeasurable, cancellationToken);

        var muscleDTOs = muscles.Select(mapper.Map<MuscleDTO>);
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
    public async Task<ActionResult<ApiResult<MuscleDTO>>> GetParentMusclesAsync(CancellationToken cancellationToken,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var parentMuscles = await muscleService.GetParentMusclesAsync(cancellationToken);

        var parentMuscleDTOs = parentMuscles.Select(mapper.Map<MuscleDTO>);
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
    public async Task<ActionResult<ApiResult<MuscleDTO>>> GetChildMusclesAsync(CancellationToken cancellationToken,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var childMuscles = await muscleService.GetChildMusclesAsync(cancellationToken);

        var childMuscleDTOs = childMuscles.Select(mapper.Map<MuscleDTO>);
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
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetExercisesByMuscleIdAsync(CancellationToken cancellationToken,
        [FromQuery] long muscleId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var muscle = await muscleService.GetMuscleByIdAsync(muscleId, userId, cancellationToken);

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
    public async Task<ActionResult<MuscleDTO>> GetMuscleByIdAsync(long muscleId, CancellationToken cancellationToken)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var muscle = await muscleService.GetMuscleByIdAsync(muscleId, userId, cancellationToken);
        return ToMuscleDTO(muscle);
    }

    [HttpGet("{muscleId}/details")]
    [ActionName(nameof(GetMuscleDetailsByIdAsync))]
    public async Task<ActionResult<MuscleDetailsDTO>> GetMuscleDetailsByIdAsync(long muscleId, CancellationToken cancellationToken)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleWithDetails = await muscleService.GetMuscleByIdWithDetailsAsync(muscleId, userId, cancellationToken);
        return ToMuscleDetailsDTO(muscleWithDetails);
    }


    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<MuscleDTO>> GetMuscleByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return MuscleNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var muscle = await muscleService.GetMuscleByNameAsync(name, userId, cancellationToken);
        return ToMuscleDTO(muscle);
    }

    [HttpGet("by-name/{name}/details")]
    public async Task<ActionResult<MuscleDetailsDTO>> GetMuscleDetailsByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return MuscleNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleWithDetails = await muscleService.GetMuscleByNameWithDetailsAsync(name, userId, cancellationToken);
        return ToMuscleDetailsDTO(muscleWithDetails);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddMuscleAsync([FromBody] MuscleCreationDTO muscleCreationDTO, CancellationToken cancellationToken)
    {
        if (muscleCreationDTO is null)
            return MuscleIsNull();

        var muscle = mapper.Map<Muscle>(muscleCreationDTO);
        muscle = await muscleService.AddMuscleAsync(muscle, cancellationToken);

        var muscleDTO = mapper.Map<MuscleDTO>(muscle);
        return CreatedAtAction(nameof(GetMuscleByIdAsync), new { muscleId = muscle.Id }, muscleDTO);
    }

    [HttpPut("{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateMuscleAsync(long muscleId, [FromBody] MuscleUpdateDTO muscleUpdateDTO, CancellationToken cancellationToken)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        if (muscleUpdateDTO is null)
            return MuscleIsNull();

        if (!AreIdsEqual(muscleId, muscleUpdateDTO.Id))
            return EntryIDsNotMatch(nameof(Muscle));

        var muscle = mapper.Map<Muscle>(muscleUpdateDTO);
        await muscleService.UpdateMuscleAsync(muscle, cancellationToken);

        return Ok();
    }

    [HttpPut("{muscleId}/children")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateMuscleChildrenAsync(long muscleId, IEnumerable<long>? muscleChildIDs, CancellationToken cancellationToken)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        if (muscleChildIDs is null)
            return EntryIsNull("Muscle child's Ids");

        await muscleService.UpdateMuscleChildrenAsync(muscleId, muscleChildIDs, cancellationToken);
        return Ok();
    }

    [HttpDelete("{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteMuscleAsync(long muscleId, CancellationToken cancellationToken)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        await muscleService.DeleteMuscleAsync(muscleId, cancellationToken);
        return Ok();
    }

    [HttpPut("muscle-photo/{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateMusclePhotoAsync(long muscleId, [FromForm] FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        await muscleService.UpdateMusclePhotoAsync(muscleId, fileUpload, cancellationToken);
        return Ok();
    }

    [HttpDelete("muscle-photo/{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteMusclePhotoAsync(long muscleId, CancellationToken cancellationToken)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        await muscleService.DeleteMusclePhotoAsync(muscleId, cancellationToken);
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
