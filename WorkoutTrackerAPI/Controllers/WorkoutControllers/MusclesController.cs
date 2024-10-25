using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Services.MuscleServices;
using Microsoft.AspNetCore.Authorization;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public class MusclesController : BaseWorkoutController<Muscle>
{
    readonly IMuscleService muscleService;
    public MusclesController(IMuscleService muscleService)
        => this.muscleService = muscleService;

    ActionResult<Muscle> HandleMuscleServiceResult(ServiceResult<Muscle> serviceResult)
        => HandleServiceResult(serviceResult, "Muscle not found.");

    ActionResult InvalidMuscleID()
        => InvalidEntryID(nameof(Muscle));
    ActionResult MuscleNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Muscle));
    ActionResult MuscleIsNull()
        => EntryIsNull(nameof(Muscle));


    [HttpGet]
    public async Task<ActionResult<ApiResult<Muscle>>> GetMusclesAsync(
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

        if (serviceResult.Model is null)
            return EntryNotFound("Muscles");

        return await ApiResult<Muscle>.CreateAsync(
            serviceResult.Model!,
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
    public async Task<ActionResult<Muscle>> GetMuscleByIdAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        var serviceResult = await muscleService.GetMuscleByIdAsync(muscleId);
        return HandleMuscleServiceResult(serviceResult);
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<Muscle>> GetMuscleByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return MuscleNameIsNullOrEmpty();

        var serviceResult = await muscleService.GetMuscleByNameAsync(name);
        return HandleMuscleServiceResult(serviceResult);
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddMuscleAsync(Muscle muscle)
    {
        if (muscle is null)
            return MuscleIsNull();

        if (muscle.Id != 0)
            return InvalidEntryIDWhileAdding(nameof(Muscle), "muscle");

        var serviceResult = await muscleService.AddMuscleAsync(muscle);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        muscle = serviceResult.Model!;

        return CreatedAtAction(nameof(GetMuscleByIdAsync), new { muscleId = muscle.Id }, muscle);
    }

    [HttpPut("{muscleId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateMuscleAsync(long muscleId, Muscle muscle)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        if (muscle is null)
            return MuscleIsNull();

        if (muscleId != muscle.Id)
            return EntryIDsNotMatch(nameof(Muscle));

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
