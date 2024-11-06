using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Services.MuscleServices;
using Microsoft.AspNetCore.Authorization;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;
using AutoMapper;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public class MusclesController : BaseWorkoutController<MuscleDTO, MuscleDTO>
{
    readonly IMuscleService muscleService;
    public MusclesController(IMuscleService muscleService, IMapper mapper) : base(mapper)
        => this.muscleService = muscleService;

    ActionResult<MuscleDTO> HandleMuscleDTOServiceResult(ServiceResult<Muscle> serviceResult)
        => HandleDTOServiceResult(serviceResult, "Muscle not found.");

    ActionResult InvalidMuscleID()
        => InvalidEntryID(nameof(Muscle));
    ActionResult MuscleNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Muscle));
    ActionResult MuscleIsNull()
        => EntryIsNull(nameof(Muscle));


    [HttpGet]
    public async Task<ActionResult<ApiResult<MuscleDTO>>> GetMusclesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await muscleService.GetMusclesAsync();

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

    [HttpGet("{muscleId}")]
    [ActionName(nameof(GetMuscleByIdAsync))]
    public async Task<ActionResult<MuscleDTO>> GetMuscleByIdAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        var serviceResult = await muscleService.GetMuscleByIdAsync(muscleId);
        return HandleMuscleDTOServiceResult(serviceResult);
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<MuscleDTO>> GetMuscleByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return MuscleNameIsNullOrEmpty();

        var serviceResult = await muscleService.GetMuscleByNameAsync(name);
        return HandleMuscleDTOServiceResult(serviceResult);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddMuscleAsync(MuscleDTO muscleDTO)
    {
        if (muscleDTO is null)
            return MuscleIsNull();

        if (muscleDTO.Id != 0)
            return InvalidEntryIDWhileAdding(nameof(Muscle), "muscle");

        var muscle = mapper.Map<Muscle>(muscleDTO);
        var serviceResult = await muscleService.AddMuscleAsync(muscle);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        muscle = serviceResult.Model!;

        return CreatedAtAction(nameof(GetMuscleByIdAsync), new { muscleId = muscle.Id }, muscle);
    }

    [HttpPut("{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateMuscleAsync(long muscleId, MuscleDTO muscleDTO)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        if (muscleDTO is null)
            return MuscleIsNull();

        if (muscleId != muscleDTO.Id)
            return EntryIDsNotMatch(nameof(Muscle));


        var muscle = mapper.Map<Muscle>(muscleDTO);
        var serviceResult = await muscleService.UpdateMuscleAsync(muscle);
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
