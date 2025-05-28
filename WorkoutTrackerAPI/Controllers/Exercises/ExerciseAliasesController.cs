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
    public async Task<ActionResult<ApiResult<ExerciseAliasDTO>>> GetUserExerciseAliasesAsync(CancellationToken cancellationToken,
        [FromQuery] long exerciseId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var exerciseAliases = await exerciseAliasService.GetExerciseAliasesByExerciseIdAsync(exerciseId, cancellationToken);

        var exerciseAliasDTOs = exerciseAliases.Select(mapper.Map<ExerciseAliasDTO>);
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
    public async Task<ActionResult<ExerciseAliasDTO>> GetExerciseAliasByIdAsync(long exerciseAliasId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseAliasId))
            return InvalidExerciseAliasID();

        var exerciseAliase = await exerciseAliasService.GetExerciseAliasByIdAsync(exerciseAliasId, cancellationToken);
        return ToExerciseAliasDTO(exerciseAliase);
    }

    [HttpGet("{name}/by-name")]
    [ActionName(nameof(GetExerciseAliasByNameAsync))]
    public async Task<ActionResult<ExerciseAliasDTO>> GetExerciseAliasByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return ExerciseAliasNameIsNullOrEmpty();

        var exerciseAliase = await exerciseAliasService.GetExerciseAliasByNameAsync(name, cancellationToken);
        return ToExerciseAliasDTO(exerciseAliase);
    }

    [HttpPost("{exerciseId}")]
    public async Task<IActionResult> AddExerciseAliasToExerciseAsync(long exerciseId, [FromBody] ExerciseAliasDTO exerciseAliasDTO, CancellationToken cancellationToken)
    {
        if (exerciseAliasDTO is null)
            return ExerciseAliasIsNull();

        if (!IsValidIDWhileAdding(exerciseAliasDTO.Id))
            return InvalidEntryIDWhileAdding(nameof(ExerciseAlias), "exercise alias");

        var exerciseAlias = mapper.Map<ExerciseAlias>(exerciseAliasDTO);
        exerciseAlias = await exerciseAliasService.AddExerciseAliasToExerciseAsync(exerciseId, exerciseAlias, cancellationToken);

        exerciseAliasDTO = mapper.Map<ExerciseAliasDTO>(exerciseAlias);
        return CreatedAtAction(nameof(GetExerciseAliasByIdAsync), new { exerciseAliasId = exerciseAlias.Id }, exerciseAliasDTO);
    }

    [HttpPut("{exerciseAliasId}")]
    public async Task<IActionResult> UpdateExerciseAliasAsync(long exerciseAliasId, [FromBody] ExerciseAliasDTO exerciseAliasDTO, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseAliasId))
            return InvalidExerciseAliasID();

        if (exerciseAliasDTO is null)
            return ExerciseAliasIsNull();

        if (!AreIdsEqual(exerciseAliasId, exerciseAliasDTO.Id))
            return EntryIDsNotMatch(nameof(ExerciseAlias));

        var exerciseAlias = mapper.Map<ExerciseAlias>(exerciseAliasDTO);
        await exerciseAliasService.UpdateExerciseAliasAsync(exerciseAlias, cancellationToken);
        return Ok();
    }

    [HttpDelete("{exerciseAliasId}")]
    public async Task<IActionResult> DeleteExerciseAliasAsync(long exerciseAliasId, CancellationToken cancellationToken)
    {
        if (!IsValidID(exerciseAliasId))
            return InvalidExerciseAliasID();

        await exerciseAliasService.DeleteExerciseAliasAsync(exerciseAliasId, cancellationToken);
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
