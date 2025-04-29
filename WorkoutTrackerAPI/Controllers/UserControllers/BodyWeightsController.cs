using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services.BodyWeightServices;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

[Route("api/body-weights")]
public class BodyWeightsController : DbModelController<BodyWeightDTO, BodyWeightDTO>
{
    readonly IHttpContextAccessor httpContextAccessor;
    readonly IBodyWeightService bodyWeightService;
    public BodyWeightsController(IBodyWeightService bodyWeightService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(mapper)
    {
        this.bodyWeightService = bodyWeightService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<BodyWeightDTO> HandleBodyWeightDTOServiceResult(ServiceResult<BodyWeight> serviceResult)
        => HandleDTOServiceResult<BodyWeight, BodyWeightDTO>(serviceResult, "Body weight not found.");

    ActionResult InvalidBodyWeightID()
        => InvalidEntryID(nameof(BodyWeight));
    ActionResult BodyWeightIsNull()
        => EntryIsNull("Body weight");

    [HttpGet("in-kilograms")]
    public async Task<ActionResult<ApiResult<BodyWeightDTO>>> GetCurrentUserBodyWeightsInKilogramsAsync(
        DateTimeRange? range = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (range is not null && range.LastDate.Date > DateTime.Now.Date)
            return BadRequest("Incorrect date.");

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetUserBodyWeightsInKilogramsAsync(userId, range);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<BodyWeight> bodyWeights)
            return EntryNotFound("Body weights");

        var bodyWeightDTOs = bodyWeights.ToList().Select(m => mapper.Map<BodyWeightDTO>(m));
        return await ApiResult<BodyWeightDTO>.CreateAsync(
            bodyWeightDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("in-pounds")]
    public async Task<ActionResult<ApiResult<BodyWeightDTO>>> GetCurrentUserBodyWeightsInPoundsAsync(
        DateTimeRange? range = null,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (range is not null && range.LastDate.Date > DateTime.Now.Date)
            return BadRequest("Incorrect date.");

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetUserBodyWeightsInPoundsAsync(userId, range);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<BodyWeight> bodyWeights)
            return EntryNotFound("Body weights");

        var bodyWeightDTOs = bodyWeights.ToList().Select(m => mapper.Map<BodyWeightDTO>(m));
        return await ApiResult<BodyWeightDTO>.CreateAsync(
            bodyWeightDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{bodyWeightId}")]
    [ActionName(nameof(GetCurrentUserBodyWeightByIdAsync))]
    public async Task<ActionResult<BodyWeightDTO>> GetCurrentUserBodyWeightByIdAsync(long bodyWeightId)
    {
        if (bodyWeightId < 1)
            return InvalidBodyWeightID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetUserBodyWeightByIdAsync(userId, bodyWeightId);
        return HandleBodyWeightDTOServiceResult(serviceResult);
    }

    [HttpGet("min-body-weight")]
    public async Task<ActionResult<BodyWeightDTO>> GetMinCurrentUserBodyWeightAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetMinUserBodyWeightAsync(userId);
        return HandleBodyWeightDTOServiceResult(serviceResult);
    }

    [HttpGet("max-body-weight")]
    public async Task<ActionResult<BodyWeightDTO>> GetMaxCurrentUserBodyWeightAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.GetMaxUserBodyWeightAsync(userId);
        return HandleBodyWeightDTOServiceResult(serviceResult);
    }

    [HttpPost]
    public async Task<IActionResult> AddBodyWeightToCurrentUserAsync([FromBody] BodyWeightDTO bodyWeightDTO)
    {
        if (bodyWeightDTO is null)
            return BodyWeightIsNull();

        if (bodyWeightDTO.Id != 0)
            return InvalidEntryIDWhileAdding(nameof(BodyWeight), "body weight");

        string userId = httpContextAccessor.GetUserId()!;
        var bodyWeight = mapper.Map<BodyWeight>(bodyWeightDTO);
        var serviceResult = await bodyWeightService.AddBodyWeightToUserAsync(userId, bodyWeight);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        bodyWeight = serviceResult.Model!;

        bodyWeightDTO = mapper.Map<BodyWeightDTO>(bodyWeight);
        return CreatedAtAction(nameof(GetCurrentUserBodyWeightByIdAsync), new { bodyWeightId = bodyWeight.Id }, bodyWeightDTO);
    }

    [HttpPut("{bodyWeightId}")]
    public async Task<IActionResult> UpdateCurrentUserBodyWeightAsync(long bodyWeightId, [FromBody] BodyWeightDTO bodyWeightDTO)
    {
        if (bodyWeightId < 1)
            return InvalidBodyWeightID();

        if (bodyWeightDTO is null)
            return BodyWeightIsNull();

        if (bodyWeightId != bodyWeightDTO.Id)
            return EntryIDsNotMatch(nameof(BodyWeight));

        string userId = httpContextAccessor.GetUserId()!;
        var bodyWeight = mapper.Map<BodyWeight>(bodyWeightDTO);
        var serviceResult = await bodyWeightService.UpdateUserBodyWeightAsync(userId, bodyWeight);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{bodyWeightId}")]
    public async Task<IActionResult> DeleteBodyWeightFromCurrentUserAsync(long bodyWeightId)
    {
        if (bodyWeightId < 1)
            return InvalidBodyWeightID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await bodyWeightService.DeleteBodyWeightFromUserAsync(userId, bodyWeightId);
        return HandleServiceResult(serviceResult);
    }
}
