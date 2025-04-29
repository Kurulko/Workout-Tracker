using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Services.ExerciseAliasServices;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.ExerciseAliasDTOs;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

[Route("api/exercise-aliases")]
public class ExerciseAliasesController : BaseWorkoutController<ExerciseAliasDTO, ExerciseAliasDTO>
{
    readonly IExerciseAliasService exerciseAliasService;
    public ExerciseAliasesController(IExerciseAliasService exerciseAliasService, IMapper mapper)
        : base(mapper)
    {
        this.exerciseAliasService = exerciseAliasService;
    }

    ActionResult<ExerciseAliasDTO> HandleExerciseAliasDTOServiceResult(ServiceResult<ExerciseAlias> serviceResult)
        => HandleDTOServiceResult<ExerciseAlias, ExerciseAliasDTO>(serviceResult, "Exercise alias not found.");

    ActionResult InvalidExerciseAliasID()
        => InvalidEntryID(nameof(ExerciseAlias));
    ActionResult ExerciseAliasIsNull()
        => EntryIsNull("Exercise alias");
    ActionResult ExerciseAliasNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(ExerciseAlias));

    [HttpGet("{exerciseId}")]
    public async Task<ActionResult<ApiResult<ExerciseAliasDTO>>> GetUserExerciseAliasesAsync(
        long exerciseId,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await exerciseAliasService.GetExerciseAliasesByExerciseIdAsync(exerciseId);
        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<ExerciseAlias> exerciseAliases)
            return EntryNotFound("Exercise aliases");

        var exerciseAliasDTOs = exerciseAliases.ToList().Select(m => mapper.Map<ExerciseAliasDTO>(m));
        return await ApiResult<ExerciseAliasDTO>.CreateAsync(
            exerciseAliasDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{exerciseAliasId}")]
    [ActionName(nameof(GetExerciseAliasByIdAsync))]
    public async Task<ActionResult<ExerciseAliasDTO>> GetExerciseAliasByIdAsync(long exerciseAliasId)
    {
        if (exerciseAliasId < 1)
            return InvalidExerciseAliasID();

        var serviceResult = await exerciseAliasService.GetExerciseAliasByIdAsync(exerciseAliasId);
        return HandleExerciseAliasDTOServiceResult(serviceResult);
    }

    [HttpGet("{name}/by-name")]
    [ActionName(nameof(GetExerciseAliasByNameAsync))]
    public async Task<ActionResult<ExerciseAliasDTO>> GetExerciseAliasByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return ExerciseAliasNameIsNullOrEmpty();

        var serviceResult = await exerciseAliasService.GetExerciseAliasByNameAsync(name);
        return HandleExerciseAliasDTOServiceResult(serviceResult);
    }

    [HttpPost("{exerciseId}")]
    public async Task<IActionResult> AddExerciseAliasToExerciseAsync(long exerciseId, [FromBody] ExerciseAliasDTO exerciseAliasDTO)
    {
        if (exerciseAliasDTO is null)
            return ExerciseAliasIsNull();

        if (exerciseAliasDTO.Id != 0)
            return InvalidEntryIDWhileAdding(nameof(ExerciseAlias), "exercise alias");

        var exerciseAlias = mapper.Map<ExerciseAlias>(exerciseAliasDTO);
        var serviceResult = await exerciseAliasService.AddExerciseAliasToExerciseAsync(exerciseId, exerciseAlias);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        exerciseAlias = serviceResult.Model!;

        exerciseAliasDTO = mapper.Map<ExerciseAliasDTO>(exerciseAlias);
        return CreatedAtAction(nameof(GetExerciseAliasByIdAsync), new { exerciseAliasId = exerciseAlias.Id }, exerciseAliasDTO);
    }

    [HttpPut("{exerciseAliasId}")]
    public async Task<IActionResult> UpdateExerciseAliasAsync(long exerciseAliasId, [FromBody] ExerciseAliasDTO exerciseAliasDTO)
    {
        if (exerciseAliasId < 1)
            return InvalidExerciseAliasID();

        if (exerciseAliasDTO is null)
            return ExerciseAliasIsNull();

        if (exerciseAliasId != exerciseAliasDTO.Id)
            return EntryIDsNotMatch(nameof(ExerciseAlias));

        var exerciseAlias = mapper.Map<ExerciseAlias>(exerciseAliasDTO);
        var serviceResult = await exerciseAliasService.UpdateExerciseAliasAsync(exerciseAlias);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{exerciseAliasId}")]
    public async Task<IActionResult> DeleteExerciseAliasAsync(long exerciseAliasId)
    {
        if (exerciseAliasId < 1)
            return InvalidExerciseAliasID();

        var serviceResult = await exerciseAliasService.DeleteExerciseAliasAsync(exerciseAliasId);
        return HandleServiceResult(serviceResult);
    }
}
