using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.DTOs.BodyWeights;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.API.Controllers;

[Route("api/body-weights")]
public class BodyWeightsController : DbModelController<BodyWeightDTO, BodyWeightDTO>
{
    readonly IHttpContextAccessor httpContextAccessor;
    readonly IBodyWeightService bodyWeightService;
    public BodyWeightsController(
        IBodyWeightService bodyWeightService, 
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.bodyWeightService = bodyWeightService;
        this.httpContextAccessor = httpContextAccessor;
    }


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
        if (range is DateTimeRange _range && IsDateInFuture(_range))
            return DateInFuture();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var bodyWeights = await bodyWeightService.GetUserBodyWeightsInKilogramsAsync(userId, range);

        var bodyWeightDTOs = bodyWeights.ToList().Select(mapper.Map<BodyWeightDTO>);
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
        if (range is DateTimeRange _range && IsDateInFuture(_range))
            return DateInFuture();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var bodyWeights = await bodyWeightService.GetUserBodyWeightsInPoundsAsync(userId, range);

        var bodyWeightDTOs = bodyWeights.ToList().Select(mapper.Map<BodyWeightDTO>);
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
        if (!IsValidID(bodyWeightId))
            return InvalidBodyWeightID();

        string userId = httpContextAccessor.GetUserId()!;
        var bodyWeight = await bodyWeightService.GetUserBodyWeightByIdAsync(userId, bodyWeightId);
        return ToBodyWeightDTO(bodyWeight);
    }

    [HttpGet("min-body-weight")]
    public async Task<ActionResult<BodyWeightDTO>> GetMinCurrentUserBodyWeightAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        var minBodyWeight = await bodyWeightService.GetMinUserBodyWeightAsync(userId);
        return ToBodyWeightDTO(minBodyWeight);
    }

    [HttpGet("max-body-weight")]
    public async Task<ActionResult<BodyWeightDTO>> GetMaxCurrentUserBodyWeightAsync()
    {
        string userId = httpContextAccessor.GetUserId()!;
        var maxBodyWeight = await bodyWeightService.GetMaxUserBodyWeightAsync(userId);
        return ToBodyWeightDTO(maxBodyWeight);
    }

    [HttpPost]
    public async Task<IActionResult> AddBodyWeightToCurrentUserAsync([FromBody] BodyWeightCreationDTO bodyWeightCreationDTO)
    {
        if (bodyWeightCreationDTO is null)
            return BodyWeightIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var bodyWeight = mapper.Map<BodyWeight>(bodyWeightCreationDTO);
        bodyWeight = await bodyWeightService.AddBodyWeightToUserAsync(userId, bodyWeight);

        var bodyWeightDTO = mapper.Map<BodyWeightDTO>(bodyWeight);
        return CreatedAtAction(nameof(GetCurrentUserBodyWeightByIdAsync), new { bodyWeightId = bodyWeight.Id }, bodyWeightDTO);
    }

    [HttpPut("{bodyWeightId}")]
    public async Task<IActionResult> UpdateCurrentUserBodyWeightAsync(long bodyWeightId, [FromBody] BodyWeightCreationDTO bodyWeightDTO)
    {
        if (!IsValidID(bodyWeightId))
            return InvalidBodyWeightID();

        if (bodyWeightDTO is null)
            return BodyWeightIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var bodyWeight = mapper.Map<BodyWeight>(bodyWeightDTO);
        bodyWeight.Id = bodyWeightId;

        await bodyWeightService.UpdateUserBodyWeightAsync(userId, bodyWeight);
        return Ok();
    }

    [HttpDelete("{bodyWeightId}")]
    public async Task<IActionResult> DeleteBodyWeightFromCurrentUserAsync(long bodyWeightId)
    {
        if (!IsValidID(bodyWeightId))
            return InvalidBodyWeightID();

        string userId = httpContextAccessor.GetUserId()!;
        await bodyWeightService.DeleteBodyWeightFromUserAsync(userId, bodyWeightId);
        return Ok();
    }


    ActionResult<BodyWeightDTO> ToBodyWeightDTO(BodyWeight? bodyWeight)
        => ToDTO<BodyWeight, BodyWeightDTO>(bodyWeight, "Body weight not found.");

    ActionResult InvalidBodyWeightID()
        => InvalidEntryID(nameof(BodyWeight));
    ActionResult BodyWeightIsNull()
        => EntryIsNull("Body weight");
}
