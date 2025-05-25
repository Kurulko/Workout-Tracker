using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseAliases;
using WorkoutTracker.Application.Interfaces.Services.Exercises;
using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.API.Controllers.Exercises;

[Route("api/exercise-aliases")]
public class ExerciseAliasesController : BaseWorkoutController<ExerciseAliasDTO, ExerciseAliasDTO>
{
    readonly IExerciseAliasService exerciseAliasService;
    public ExerciseAliasesController(IExerciseAliasService exerciseAliasService, IMapper mapper)
        : base(mapper)
    {
        this.exerciseAliasService = exerciseAliasService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResult<ExerciseAliasDTO>>> GetUserExerciseAliasesAsync(
        long exerciseId,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var exerciseAliases = await exerciseAliasService.GetExerciseAliasesByExerciseIdAsync(exerciseId);

        var exerciseAliasDTOs = exerciseAliases.ToList().Select(mapper.Map<ExerciseAliasDTO>);
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
        if (!IsValidID(exerciseAliasId))
            return InvalidExerciseAliasID();

        var exerciseAliase = await exerciseAliasService.GetExerciseAliasByIdAsync(exerciseAliasId);
        return ToExerciseAliasDTO(exerciseAliase);
    }

    [HttpGet("{name}/by-name")]
    [ActionName(nameof(GetExerciseAliasByNameAsync))]
    public async Task<ActionResult<ExerciseAliasDTO>> GetExerciseAliasByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return ExerciseAliasNameIsNullOrEmpty();

        var exerciseAliase = await exerciseAliasService.GetExerciseAliasByNameAsync(name);
        return ToExerciseAliasDTO(exerciseAliase);
    }

    [HttpPost("{exerciseId}")]
    public async Task<IActionResult> AddExerciseAliasToExerciseAsync(long exerciseId, [FromBody] ExerciseAliasDTO exerciseAliasDTO)
    {
        if (exerciseAliasDTO is null)
            return ExerciseAliasIsNull();

        if (!IsValidIDWhileAdding(exerciseAliasDTO.Id))
            return InvalidEntryIDWhileAdding(nameof(ExerciseAlias), "exercise alias");

        var exerciseAlias = mapper.Map<ExerciseAlias>(exerciseAliasDTO);
        exerciseAlias = await exerciseAliasService.AddExerciseAliasToExerciseAsync(exerciseId, exerciseAlias);

        exerciseAliasDTO = mapper.Map<ExerciseAliasDTO>(exerciseAlias);
        return CreatedAtAction(nameof(GetExerciseAliasByIdAsync), new { exerciseAliasId = exerciseAlias.Id }, exerciseAliasDTO);
    }

    [HttpPut("{exerciseAliasId}")]
    public async Task<IActionResult> UpdateExerciseAliasAsync(long exerciseAliasId, [FromBody] ExerciseAliasDTO exerciseAliasDTO)
    {
        if (!IsValidID(exerciseAliasId))
            return InvalidExerciseAliasID();

        if (exerciseAliasDTO is null)
            return ExerciseAliasIsNull();

        if (!AreIdsEqual(exerciseAliasId, exerciseAliasDTO.Id))
            return EntryIDsNotMatch(nameof(ExerciseAlias));

        var exerciseAlias = mapper.Map<ExerciseAlias>(exerciseAliasDTO);
        await exerciseAliasService.UpdateExerciseAliasAsync(exerciseAlias);
        return Ok();
    }

    [HttpDelete("{exerciseAliasId}")]
    public async Task<IActionResult> DeleteExerciseAliasAsync(long exerciseAliasId)
    {
        if (!IsValidID(exerciseAliasId))
            return InvalidExerciseAliasID();

        await exerciseAliasService.DeleteExerciseAliasAsync(exerciseAliasId);
        return Ok();
    }


    ActionResult<ExerciseAliasDTO> ToExerciseAliasDTO(ExerciseAlias? exerciseAlias)
        => ToDTO<ExerciseAlias, ExerciseAliasDTO>(exerciseAlias, "Exercise alias not found.");

    ActionResult InvalidExerciseAliasID()
        => InvalidEntryID(nameof(ExerciseAlias));
    ActionResult ExerciseAliasIsNull()
        => EntryIsNull("Exercise alias");
    ActionResult ExerciseAliasNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(ExerciseAlias));
}
