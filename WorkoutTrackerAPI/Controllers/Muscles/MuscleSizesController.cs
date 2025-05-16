using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.API.Controllers.Muscles;

[Route("api/muscle-sizes")]
public class MuscleSizesController : DbModelController<MuscleSizeDTO, MuscleSizeDTO>
{
    readonly IMuscleSizeService muscleSizeService;
    readonly IHttpContextAccessor httpContextAccessor;
    public MuscleSizesController(IMuscleSizeService muscleSizeService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(mapper)
    {
        this.muscleSizeService = muscleSizeService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<MuscleSizeDTO> HandleMuscleSizeDTOServiceResult(ServiceResult<MuscleSize> serviceResult)
        => HandleDTOServiceResult<MuscleSize, MuscleSizeDTO>(serviceResult, "Muscle size not found.");

    ActionResult InvalidMuscleSizeID()
        => InvalidEntryID(nameof(MuscleSize));
    ActionResult InvalidMuscleID()
        => InvalidEntryID(nameof(Muscle));
    ActionResult MuscleSizeIsNull()
        => EntryIsNull("Muscle size");

    
    [HttpGet("in-centimeters")]
    public async Task<ActionResult<ApiResult<MuscleSizeDTO>>> GetCurrentUserMuscleSizesInCentimetersAsync(
        [FromQuery] long? muscleId = null, 
        [FromQuery] DateTimeRange? range = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (range is DateTimeRange _range && _range.LastDate > DateTime.Now.Date)
            return BadRequest("Incorrect date.");

        if (muscleId.HasValue && muscleId < 1)
            return InvalidEntryID(nameof(Muscle));

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetUserMuscleSizesInCentimetersAsync(userId, muscleId, range);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<MuscleSize> muscleSizes)
            return EntryNotFound("Muscle sizes");

        var muscleSizeDTOs = muscleSizes.ToList().Select(m => mapper.Map<MuscleSizeDTO>(m));
        return await ApiResult<MuscleSizeDTO>.CreateAsync(
            muscleSizeDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }
    
    [HttpGet("in-inches")]
    public async Task<ActionResult<ApiResult<MuscleSizeDTO>>> GetCurrentUserMuscleSizesInInchesAsync(
        [FromQuery] long? muscleId = null,
        [FromQuery] DateTimeRange? range = null,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery]  string? filterQuery = null)
    {
        if (range is DateTimeRange _range && _range.LastDate > DateTime.Now.Date)
            return BadRequest("Incorrect date.");

        if (muscleId.HasValue && muscleId < 1)
            return InvalidEntryID(nameof(Muscle));

        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetUserMuscleSizesInInchesAsync(userId, muscleId, range);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<MuscleSize> muscleSizes)
            return EntryNotFound("Muscle sizes");

        var muscleSizeDTOs = muscleSizes.ToList().Select(mapper.Map<MuscleSizeDTO>);
        return await ApiResult<MuscleSizeDTO>.CreateAsync(
            muscleSizeDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{muscleSizeId}")]
    [ActionName(nameof(GetCurrentUserMuscleSizeByIdAsync))]
    public async Task<ActionResult<MuscleSizeDTO>> GetCurrentUserMuscleSizeByIdAsync(long muscleSizeId)
    {
        if (muscleSizeId < 1)
            return InvalidMuscleSizeID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetUserMuscleSizeByIdAsync(userId, muscleSizeId);
        return HandleMuscleSizeDTOServiceResult(serviceResult);
    }

    [HttpGet("min-muscle-size")]
    public async Task<ActionResult<MuscleSizeDTO>> GetMinCurrentUserMuscleSizeAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetMinUserMuscleSizeAsync(userId, muscleId);
        return HandleMuscleSizeDTOServiceResult(serviceResult);
    }

    [HttpGet("max-muscle-size")]
    public async Task<ActionResult<MuscleSizeDTO>> GetMaxCurrentUserMuscleSizeAsync(long muscleId)
    {
        if (muscleId < 1)
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.GetMaxUserMuscleSizeAsync(userId, muscleId);
        return HandleMuscleSizeDTOServiceResult(serviceResult);
    }


    [HttpPost]
    public async Task<IActionResult> AddMuscleSizeToCurrentUserAsync(MuscleSizeCreationDTO muscleSizeCreationDTO)
    {
        if (muscleSizeCreationDTO is null)
            return MuscleSizeIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleSize = mapper.Map<MuscleSize>(muscleSizeCreationDTO);
        var serviceResult = await muscleSizeService.AddMuscleSizeToUserAsync(userId, muscleSize);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        muscleSize = serviceResult.Model!;

        var muscleSizeDTO = mapper.Map<MuscleSizeDTO>(muscleSize);
        return CreatedAtAction(nameof(GetCurrentUserMuscleSizeByIdAsync), new { muscleSizeId = muscleSize.Id }, muscleSizeDTO);
    }

    [HttpPut("{muscleSizeId}")]
    public async Task<IActionResult> UpdateCurrentUserMuscleSizeAsync(long muscleSizeId, MuscleSizeCreationDTO muscleSizeDTO)
    {
        if(muscleSizeId < 1)
            return InvalidMuscleSizeID();

        if (muscleSizeDTO is null)
            return MuscleSizeIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleSize = mapper.Map<MuscleSize>(muscleSizeDTO);
        muscleSize.Id = muscleSizeId;

        var serviceResult = await muscleSizeService.UpdateUserMuscleSizeAsync(userId, muscleSize);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{muscleSizeId}")]
    public async Task<IActionResult> DeleteMuscleSizeFromCurrentUserAsync(long muscleSizeId)
    {
        if (muscleSizeId < 1)
            return InvalidMuscleSizeID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await muscleSizeService.DeleteMuscleSizeFromUserAsync(userId, muscleSizeId);
        return HandleServiceResult(serviceResult);
    }
}
