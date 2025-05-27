using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.DTOs;
using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSetGroups;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;
using WorkoutTracker.Application.DTOs.Workouts.Workouts;
using WorkoutTracker.Application.Interfaces.Services.Workouts;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Application.Common.Extensions;

namespace WorkoutTracker.API.Controllers.Workouts;

public class WorkoutsController : BaseWorkoutController<WorkoutDTO, WorkoutCreationDTO>
{
    readonly IWorkoutService workoutService;
    readonly IHttpContextAccessor httpContextAccessor;
    public WorkoutsController (
        IWorkoutService workoutService, 
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.workoutService = workoutService;
        this.httpContextAccessor = httpContextAccessor;
    }


    [HttpGet]
    public async Task<ActionResult<ApiResult<WorkoutDTO>>> GetCurrentUserWorkoutsAsync(CancellationToken cancellationToken,
        [FromQuery] long? exerciseId = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        if (exerciseId.HasValue && !IsValidID(exerciseId.Value))
            return InvalidEntryID(nameof(Exercise));

        string userId = httpContextAccessor.GetUserId()!;
        var workouts = await workoutService.GetUserWorkoutsAsync(userId, exerciseId, cancellationToken);

        var workoutDTOs = workouts.Select(mapper.Map<WorkoutDTO>);
        return await ApiResult<WorkoutDTO>.CreateAsync(
            workoutDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }


    [HttpGet("{workoutId}")]
    [ActionName(nameof(GetCurrentUserWorkoutByIdAsync))]
    public async Task<ActionResult<WorkoutDTO>> GetCurrentUserWorkoutByIdAsync(long workoutId, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutId))
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        var workout = await workoutService.GetUserWorkoutByIdAsync(userId, workoutId, cancellationToken);
        return ToWorkoutDTO(workout);
    }

    [HttpGet("{workoutId}/details")]
    [ActionName(nameof(GetCurrentUserWorkoutDetailsByIdAsync))]
    public async Task<ActionResult<WorkoutDetailsDTO>> GetCurrentUserWorkoutDetailsByIdAsync(long workoutId, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutId))
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        var workoutWithDetails = await workoutService.GetUserWorkoutByIdWithDetailsAsync(userId, workoutId, cancellationToken);
        return ToWorkoutDetailsDTO(workoutWithDetails);
    }


    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<WorkoutDTO>> GetCurrentUserWorkoutByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return WorkoutNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var workout = await workoutService.GetUserWorkoutByNameAsync(userId, name, cancellationToken);
        return ToWorkoutDTO(workout);
    }

    [HttpGet("by-name/{name}/details")]
    public async Task<ActionResult<WorkoutDetailsDTO>> GetCurrentUserWorkoutDetailsByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return WorkoutNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var workoutWithDetails = await workoutService.GetUserWorkoutByNameWithDetailsAsync(userId, name, cancellationToken);
        return ToWorkoutDetailsDTO(workoutWithDetails);
    }

    [HttpPost]
    public async Task<IActionResult> AddCurrentUserWorkoutAsync(WorkoutCreationDTO workoutCreationDTO, CancellationToken cancellationToken)
    {
        if (workoutCreationDTO is null)
            return WorkoutIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var workout = mapper.Map<Workout>(workoutCreationDTO);
        workout = await workoutService.AddUserWorkoutAsync(userId, workout, cancellationToken);

        var workoutDTO = mapper.Map<WorkoutDTO>(workout);
        return CreatedAtAction(nameof(GetCurrentUserWorkoutByIdAsync), new { workoutId = workout.Id }, workoutDTO);
    }

    [HttpPost("exercise-set-groups/{workoutId}")]
    public async Task<IActionResult> AddExerciseSetGroupsToCurrentUserWorkoutAsync([FromRoute] long workoutId, [FromBody] IEnumerable<ExerciseSetGroupCreationDTO> exerciseSetGroupCreationDTOs, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutId))
            return InvalidWorkoutID();

        if (exerciseSetGroupCreationDTOs is null)
            return EntryIsNull("Exercise Set Groups");

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseSetGroups = exerciseSetGroupCreationDTOs.ToList().Select(mapper.Map<ExerciseSetGroup>);
        await workoutService.AddExerciseSetGroupsToUserWorkoutAsync(userId, workoutId, exerciseSetGroups, cancellationToken);
        
        return Ok();
    }

    [HttpPut("exercise-set-groups/{workoutId}")]
    public async Task<IActionResult> UpdateCurrentUserWorkoutExerciseSetGroupsAsync([FromRoute] long workoutId, [FromBody] IEnumerable<ExerciseSetGroupCreationDTO> exerciseSetGroupCreationDTOs, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutId))
            return InvalidWorkoutID();

        if (exerciseSetGroupCreationDTOs is null)
            return EntryIsNull("Exercise Set Groups");

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseSetGroups = exerciseSetGroupCreationDTOs.ToList().Select(mapper.Map<ExerciseSetGroup>);
        await workoutService.UpdateUserWorkoutExerciseSetGroupsAsync(userId, workoutId, exerciseSetGroups, cancellationToken);

        return Ok();
    }

    [HttpPut("complete/{workoutId}")]
    public async Task<IActionResult> CompleteCurrentUserWorkout([FromRoute] long workoutId, [FromBody] DateAndTime dateAndTime, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutId))
            return InvalidWorkoutID();

        if (IsDateInFuture(dateAndTime))
            return DateInFuture();

        string userId = httpContextAccessor.GetUserId()!;
        await workoutService.CompleteUserWorkout(userId, workoutId, dateAndTime.Date, (TimeSpan)dateAndTime.Time, cancellationToken);
        return Ok();
    }

    [HttpPut("pin/{workoutId}")]
    public async Task<IActionResult> PinCurrentUserWorkout(long workoutId, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutId))
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        await workoutService.PinUserWorkout(userId, workoutId, cancellationToken);
        return Ok();
    }

    [HttpPut("unpin/{workoutId}")]
    public async Task<IActionResult> UnpinCurrentUserWorkout(long workoutId, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutId))
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        await workoutService.UnpinUserWorkout(userId, workoutId, cancellationToken);
        return Ok();
    }

    [HttpPut("{workoutId}")]
    public async Task<IActionResult> UpdateCurrentUserWorkoutAsync(long workoutId, WorkoutDTO workoutDTO, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutId))
            return InvalidWorkoutID();

        if (workoutDTO is null)
            return WorkoutIsNull();

        if (!AreIdsEqual(workoutId, workoutDTO.Id))
            return EntryIDsNotMatch(nameof(Workout));

        string userId = httpContextAccessor.GetUserId()!;
        var workout = mapper.Map<Workout>(workoutDTO);
        await workoutService.UpdateUserWorkoutAsync(userId, workout, cancellationToken);

        return Ok();
    }

    [HttpDelete("{workoutId}")]
    public async Task<IActionResult> DeleteCurrentUserWorkoutAsync(long workoutId, CancellationToken cancellationToken)
    {
        if (!IsValidID(workoutId))
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        await workoutService.DeleteUserWorkoutAsync(userId, workoutId, cancellationToken);
        return Ok();
    }


    const string workoutNotFoundStr = "Workout not found.";
    ActionResult<WorkoutDTO> ToWorkoutDTO(Workout? workout)
        => ToDTO<Workout, WorkoutDTO>(workout, workoutNotFoundStr);
    ActionResult<WorkoutDetailsDTO> ToWorkoutDetailsDTO(Workout? workout)
    {
        if(workout == null)
            return null!;

        var workoutDetailsDTO = GetWorkoutDetailsDTO(workout);
        return workoutDetailsDTO;
    }

    WorkoutDetailsDTO GetWorkoutDetailsDTO(Workout workout)
    {
        var workoutDetailsDTO = new WorkoutDetailsDTO();

        var workoutDTO = mapper.Map<WorkoutDTO>(workout);
        workoutDetailsDTO.Workout = workoutDTO;

        var workoutRecords = workout.WorkoutRecords!;
        var totalWorkouts = workoutRecords.Count();

        if (totalWorkouts > 0)
        {
            var totalWeight = workoutRecords.GetTotalWeightValue();
            var totalDuration = workoutRecords.GetTotalTime();
            var averageWorkoutDuration = TimeSpan.FromMinutes(totalDuration.TotalMinutes / totalWorkouts);
            var dates = workoutRecords.Select(wr => wr.Date).Distinct().ToList();

            workoutDetailsDTO.TotalWorkouts = totalWorkouts;
            workoutDetailsDTO.TotalWeight = totalWeight;
            workoutDetailsDTO.TotalDuration = (TimeSpanModel)totalDuration;
            workoutDetailsDTO.AverageWorkoutDuration = (TimeSpanModel)averageWorkoutDuration;
            workoutDetailsDTO.Dates = dates;

            if (workoutDTO.Started.HasValue)
                workoutDetailsDTO.CountOfDaysSinceFirstWorkout = (int)(DateTime.Now - workoutDTO.Started.Value).TotalDays;
        }

        var muscles = workout.ExerciseSetGroups!.GetMuscles();
        var equipments = workout.ExerciseSetGroups!.GetEquipments();

        workoutDetailsDTO.Muscles = muscles.Select(mapper.Map<MuscleDTO>);
        workoutDetailsDTO.Equipments = equipments.Select(mapper.Map<EquipmentDTO>);

        return workoutDetailsDTO;
    }

    ActionResult InvalidWorkoutID()
        => InvalidEntryID(nameof(Workout));
    ActionResult WorkoutNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Workout));
    ActionResult WorkoutIsNull()
        => EntryIsNull(nameof(Workout));
}
