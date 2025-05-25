using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;
using WorkoutTracker.Application.Interfaces.Services.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.API.Controllers.Muscles;

[Route("api/muscle-sizes")]
public class MuscleSizesController : DbModelController<MuscleSizeDTO, MuscleSizeDTO>
{
    readonly IMuscleSizeService muscleSizeService;
    readonly IHttpContextAccessor httpContextAccessor;
    public MuscleSizesController(
        IMuscleSizeService muscleSizeService,
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.muscleSizeService = muscleSizeService;
        this.httpContextAccessor = httpContextAccessor;
    }


    [HttpGet("in-centimeters")]
    public async Task<ActionResult<ApiResult<MuscleSizeDTO>>> GetCurrentUserMuscleSizesInCentimetersAsync(
        long? muscleId = null,
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

        if (muscleId.HasValue && !IsValidID(muscleId.Value))
            return InvalidMuscleID();

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleSizesInCentimeters = await muscleSizeService.GetUserMuscleSizesInCentimetersAsync(userId, muscleId, range);

        var muscleSizeDTOs = muscleSizesInCentimeters.ToList().Select(mapper.Map<MuscleSizeDTO>);
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
        long? muscleId = null,
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

        if (muscleId.HasValue && !IsValidID(muscleId.Value))
            return InvalidEntryID(nameof(Muscle));

        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleSizesInInches = await muscleSizeService.GetUserMuscleSizesInInchesAsync(userId, muscleId, range);

        var muscleSizeDTOs = muscleSizesInInches.ToList().Select(mapper.Map<MuscleSizeDTO>);
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
        if (!IsValidID(muscleSizeId))
            return InvalidMuscleSizeID();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleSize = await muscleSizeService.GetUserMuscleSizeByIdAsync(userId, muscleSizeId);
        return ToMuscleSizeDTO(muscleSize);
    }

    [HttpGet("min-muscle-size")]
    public async Task<ActionResult<MuscleSizeDTO>> GetMinCurrentUserMuscleSizeAsync(long muscleId)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var minMuscleSize = await muscleSizeService.GetMinUserMuscleSizeAsync(userId, muscleId);
        return ToMuscleSizeDTO(minMuscleSize);
    }

    [HttpGet("max-muscle-size")]
    public async Task<ActionResult<MuscleSizeDTO>> GetMaxCurrentUserMuscleSizeAsync(long muscleId)
    {
        if (!IsValidID(muscleId))
            return InvalidMuscleID();

        string userId = httpContextAccessor.GetUserId()!;
        var maxMuscleSize = await muscleSizeService.GetMaxUserMuscleSizeAsync(userId, muscleId);
        return ToMuscleSizeDTO(maxMuscleSize);
    }


    [HttpPost]
    public async Task<IActionResult> AddMuscleSizeToCurrentUserAsync(MuscleSizeCreationDTO muscleSizeCreationDTO)
    {
        if (muscleSizeCreationDTO is null)
            return MuscleSizeIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleSize = mapper.Map<MuscleSize>(muscleSizeCreationDTO);
        muscleSize = await muscleSizeService.AddMuscleSizeToUserAsync(userId, muscleSize);

        var muscleSizeDTO = mapper.Map<MuscleSizeDTO>(muscleSize);
        return CreatedAtAction(nameof(GetCurrentUserMuscleSizeByIdAsync), new { muscleSizeId = muscleSize.Id }, muscleSizeDTO);
    }

    [HttpPut("{muscleSizeId}")]
    public async Task<IActionResult> UpdateCurrentUserMuscleSizeAsync(long muscleSizeId, MuscleSizeCreationDTO muscleSizeDTO)
    {
        if (!IsValidID(muscleSizeId))
            return InvalidMuscleSizeID();

        if (muscleSizeDTO is null)
            return MuscleSizeIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var muscleSize = mapper.Map<MuscleSize>(muscleSizeDTO);
        muscleSize.Id = muscleSizeId;

        await muscleSizeService.UpdateUserMuscleSizeAsync(userId, muscleSize);
        return Ok();
    }

    [HttpDelete("{muscleSizeId}")]
    public async Task<IActionResult> DeleteMuscleSizeFromCurrentUserAsync(long muscleSizeId)
    {
        if (!IsValidID(muscleSizeId))
            return InvalidMuscleSizeID();

        string userId = httpContextAccessor.GetUserId()!;
        await muscleSizeService.DeleteMuscleSizeFromUserAsync(userId, muscleSizeId);
        return Ok();
    }


    ActionResult<MuscleSizeDTO> ToMuscleSizeDTO(MuscleSize? exerciseRecord)
        => ToDTO<MuscleSize, MuscleSizeDTO>(exerciseRecord, "Muscle size not found.");

    ActionResult InvalidMuscleSizeID()
        => InvalidEntryID(nameof(MuscleSize));
    ActionResult InvalidMuscleID()
        => InvalidEntryID(nameof(Muscle));
    ActionResult MuscleSizeIsNull()
        => EntryIsNull("Muscle size");
}
