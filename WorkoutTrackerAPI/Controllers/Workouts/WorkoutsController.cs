using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.Common.Results;
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
    public WorkoutsController(IWorkoutService workoutService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(mapper)
    {
        this.workoutService = workoutService;
        this.httpContextAccessor = httpContextAccessor;
    }


    const string workoutNotFoundStr = "Workout not found.";
    ActionResult<WorkoutDTO> HandleWorkoutDTOServiceResult(ServiceResult<Workout> serviceResult)
        => HandleDTOServiceResult<Workout, WorkoutDTO>(serviceResult, workoutNotFoundStr);
    ActionResult<WorkoutDetailsDTO> HandleWorkoutDetailsDTOServiceResult(ServiceResult<Workout> serviceResult)
    {
        ServiceResult<WorkoutDetailsDTO> serviceResultDTO;

        if (serviceResult.Success)
        {
            var workoutDetailsDTO = GetWorkoutDetailsDTO(serviceResult.Model);
            serviceResultDTO = ServiceResult<WorkoutDetailsDTO>.Ok(workoutDetailsDTO);
        }
        else
        {
            serviceResultDTO = ServiceResult<WorkoutDetailsDTO>.Fail(serviceResult.ErrorMessage!);
        }

        return HandleServiceResult(serviceResultDTO, workoutNotFoundStr);
    }

    WorkoutDetailsDTO? GetWorkoutDetailsDTO(Workout? workout)
    {
        if (workout == null)
            return null;

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


    [HttpGet]
    public async Task<ActionResult<ApiResult<WorkoutDTO>>> GetCurrentUserWorkoutsAsync(
        long? exerciseId = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        if (exerciseId.HasValue && exerciseId < 1)
            return InvalidEntryID(nameof(Exercise));

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.GetUserWorkoutsAsync(userId, exerciseId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Workout> workouts)
            return EntryNotFound("Workouts");

        var workoutDTOs = workouts.ToList().Select(mapper.Map<WorkoutDTO>);
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
    public async Task<ActionResult<WorkoutDTO>> GetCurrentUserWorkoutByIdAsync(long workoutId)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.GetUserWorkoutByIdAsync(userId, workoutId);
        return HandleWorkoutDTOServiceResult(serviceResult);
    }

    [HttpGet("{workoutId}/details")]
    [ActionName(nameof(GetCurrentUserWorkoutDetailsByIdAsync))]
    public async Task<ActionResult<WorkoutDetailsDTO>> GetCurrentUserWorkoutDetailsByIdAsync(long workoutId)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.GetUserWorkoutByIdAsync(userId, workoutId, true);
        return HandleWorkoutDetailsDTOServiceResult(serviceResult);
    }


    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<WorkoutDTO>> GetCurrentUserWorkoutByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return WorkoutNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.GetUserWorkoutByNameAsync(userId, name);
        return HandleWorkoutDTOServiceResult(serviceResult);
    }

    [HttpGet("by-name/{name}/details")]
    public async Task<ActionResult<WorkoutDetailsDTO>> GetCurrentUserWorkoutDetailsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return WorkoutNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.GetUserWorkoutByNameAsync(userId, name, true);
        return HandleWorkoutDetailsDTOServiceResult(serviceResult);
    }

    [HttpPost]
    public async Task<IActionResult> AddCurrentUserWorkoutAsync(WorkoutCreationDTO workoutCreationDTO)
    {
        if (workoutCreationDTO is null)
            return WorkoutIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var workout = mapper.Map<Workout>(workoutCreationDTO);
        var serviceResult = await workoutService.AddUserWorkoutAsync(userId, workout);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        workout = serviceResult.Model!;
        var workoutDTO = mapper.Map<WorkoutDTO>(workout);

        return CreatedAtAction(nameof(GetCurrentUserWorkoutByIdAsync), new { workoutId = workout.Id }, workoutDTO);
    }

    [HttpPost("exercise-set-groups/{workoutId}")]
    public async Task<IActionResult> AddExerciseSetGroupsToCurrentUserWorkoutAsync([FromRoute] long workoutId, [FromBody] IEnumerable<ExerciseSetGroupCreationDTO> exerciseSetGroupCreationDTOs)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        if (exerciseSetGroupCreationDTOs is null)
            return EntryIsNull("Exercise Set Groups");

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseSetGroups = exerciseSetGroupCreationDTOs.ToList().Select(mapper.Map<ExerciseSetGroup>);
        var serviceResult = await workoutService.AddExerciseSetGroupsToUserWorkoutAsync(userId, workoutId, exerciseSetGroups);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("exercise-set-groups/{workoutId}")]
    public async Task<IActionResult> UpdateCurrentUserWorkoutExerciseSetGroupsAsync([FromRoute] long workoutId, [FromBody] IEnumerable<ExerciseSetGroupCreationDTO> exerciseSetGroupCreationDTOs)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        if (exerciseSetGroupCreationDTOs is null)
            return EntryIsNull("Exercise Set Groups");

        string userId = httpContextAccessor.GetUserId()!;
        var exerciseSetGroups = exerciseSetGroupCreationDTOs.ToList().Select(mapper.Map<ExerciseSetGroup>);
        var serviceResult = await workoutService.UpdateUserWorkoutExerciseSetGroupsAsync(userId, workoutId, exerciseSetGroups);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("complete/{workoutId}")]
    public async Task<IActionResult> CompleteCurrentUserWorkout([FromRoute] long workoutId, [FromBody] DateAndTime dateAndTime)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        if (dateAndTime.Date.Date > DateTime.Now.Date)
            return BadRequest("Incorrect date.");

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.CompleteUserWorkout(userId, workoutId, dateAndTime.Date, (TimeSpan)dateAndTime.Time);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("pin/{workoutId}")]
    public async Task<IActionResult> PinCurrentUserWorkout([FromRoute] long workoutId)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();


        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.PinUserWorkout(userId, workoutId);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("unpin/{workoutId}")]
    public async Task<IActionResult> UnpinCurrentUserWorkout([FromRoute] long workoutId)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();


        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.UnpinUserWorkout(userId, workoutId);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("{workoutId}")]
    public async Task<IActionResult> UpdateCurrentUserWorkoutAsync(long workoutId, WorkoutDTO workoutDTO)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        if (workoutDTO is null)
            return WorkoutIsNull();

        if (workoutId != workoutDTO.Id)
            return EntryIDsNotMatch(nameof(Workout));

        string userId = httpContextAccessor.GetUserId()!;
        var workout = mapper.Map<Workout>(workoutDTO);
        var serviceResult = await workoutService.UpdateUserWorkoutAsync(userId, workout);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{workoutId}")]
    public async Task<IActionResult> DeleteCurrentUserWorkoutAsync(long workoutId)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await workoutService.DeleteUserWorkoutAsync(userId, workoutId);
        return HandleServiceResult(serviceResult);
    }

    [HttpGet("workout-exists/{workoutId}")]
    public async Task<ActionResult<bool>> CurrentUserWorkoutExistsAsync(long workoutId)
    {
        if (workoutId < 1)
            return InvalidWorkoutID();

        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            return await workoutService.UserWorkoutExistsAsync(userId, workoutId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("workout-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> CurrentUserWorkoutExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return WorkoutNameIsNullOrEmpty();

        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            return await workoutService.UserWorkoutExistsByNameAsync(userId, name);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
