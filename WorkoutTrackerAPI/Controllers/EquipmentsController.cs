using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.API.Models.Requests;
using WorkoutTracker.Application.Common.Models;

namespace WorkoutTracker.API.Controllers;

public class EquipmentsController : BaseWorkoutController<EquipmentDTO, EquipmentDTO>
{
    readonly IEquipmentService equipmentService;
    readonly IHttpContextAccessor httpContextAccessor;
    public EquipmentsController (
        IEquipmentService equipmentService, 
        IHttpContextAccessor httpContextAccessor,
        IMapper mapper
    ) : base(mapper)
    {
        this.equipmentService = equipmentService;
        this.httpContextAccessor = httpContextAccessor;
    }


    #region Internal Equipments

    [HttpGet("internal-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetInternalEquipmentsAsync(CancellationToken cancellationToken,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var equipments = await equipmentService.GetInternalEquipmentsAsync(cancellationToken);

        var equipmentDTOs = equipments.Select(mapper.Map<EquipmentDTO>);
        return await ApiResult<EquipmentDTO>.CreateAsync(
            equipmentDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("internal-equipment/{equipmentId}")]
    [ActionName(nameof(GetInternalEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDTO>> GetInternalEquipmentByIdAsync(long equipmentId, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        var internalEquipment = await equipmentService.GetInternalEquipmentByIdAsync(equipmentId, cancellationToken);
        return ToEquipmentDTO(internalEquipment);
    }

    [HttpGet("internal-equipment/{equipmentId}/details")]
    [ActionName(nameof(GetInternalEquipmentDetailsByIdAsync))]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetInternalEquipmentDetailsByIdAsync(long equipmentId, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        var internalEquipmentWithDetails = await equipmentService.GetInternalEquipmentByIdWithDetailsAsync(equipmentId, cancellationToken);
        return ToEquipmentDetailsDTO(internalEquipmentWithDetails);
    }

    [HttpGet("internal-equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetInternalEquipmentByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        var userEquipment = await equipmentService.GetInternalEquipmentByNameAsync(name, cancellationToken);
        return ToEquipmentDTO(userEquipment);
    }

    [HttpGet("internal-equipment/by-name/{name}/details")]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetInternalEquipmentDetailsByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        var userEquipmentDetails = await equipmentService.GetInternalEquipmentByNameWithDetailsAsync(name, cancellationToken);
        return ToEquipmentDetailsDTO(userEquipmentDetails);
    }

    [HttpPost("internal-equipment")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddInternalEquipmentAsync([FromForm] EquipmentCreationDTO equipmentCreationDTO, CancellationToken cancellationToken)
    {
        if (equipmentCreationDTO is null)
            return EquipmentIsNull();

        var equipment = mapper.Map<Equipment>(equipmentCreationDTO);
        equipment = await equipmentService.AddInternalEquipmentAsync(equipment, cancellationToken);

        var equipmentDTO = mapper.Map<EquipmentDTO>(equipment);
        return CreatedAtAction(nameof(GetInternalEquipmentByIdAsync), new { equipmentId = equipment.Id }, equipmentDTO);
    }

    [HttpPut("internal-equipment/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalEquipmentAsync(long equipmentId, [FromForm] EquipmentUpdateDTO equipmentUpdateDTO, CancellationToken cancellationToken)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        if (equipmentUpdateDTO is null)
            return EquipmentIsNull();

        if (!AreIdsEqual(equipmentId, equipmentUpdateDTO.Id))
            return EquipmentIDsNotMatch();

        var equipment = mapper.Map<Equipment>(equipmentUpdateDTO);
        await equipmentService.UpdateInternalEquipmentAsync(equipment, cancellationToken);

        return Ok();
    }

    [HttpDelete("internal-equipment/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteInternalEquipmentAsync(long equipmentId, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        await equipmentService.DeleteInternalEquipmentAsync(equipmentId, cancellationToken);
        return Ok();
    }

    [HttpPut("internal-equipment-photo/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalEquipmentPhotoAsync(long equipmentId, [FromForm] FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        await equipmentService.UpdateInternalEquipmentPhotoAsync(equipmentId, fileUpload, cancellationToken);
        return Ok();
    }

    [HttpDelete("internal-equipment-photo/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteInternalEquipmentPhotoAsync(long equipmentId, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        await equipmentService.DeleteInternalEquipmentPhotoAsync(equipmentId, cancellationToken);
        return Ok();
    }


    #endregion

    #region User Equipments

    [HttpGet("user-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetCurrentUserEquipmentsAsync(CancellationToken cancellationToken,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipments = await equipmentService.GetUserEquipmentsAsync(userId, cancellationToken);

        var equipmentDTOs = userEquipments.Select(mapper.Map<EquipmentDTO>);
        return await ApiResult<EquipmentDTO>.CreateAsync(
            equipmentDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("user-equipment/{equipmentId}")]
    [ActionName(nameof(GetCurrentUserEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDTO>> GetCurrentUserEquipmentByIdAsync(long equipmentId, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipment = await equipmentService.GetUserEquipmentByIdAsync(userId, equipmentId, cancellationToken);
        return ToEquipmentDTO(userEquipment);
    }

    [HttpGet("user-equipment/{equipmentId}/details")]
    [ActionName(nameof(GetCurrentUserEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetCurrentUserEquipmentDetailsByIdAsync(long equipmentId, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipmentWithDetails = await equipmentService.GetUserEquipmentByIdWithDetailsAsync(userId, equipmentId, cancellationToken);
        return ToEquipmentDetailsDTO(userEquipmentWithDetails);
    }


    [HttpGet("user-equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetCurrentUserEquipmentByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipment = await equipmentService.GetUserEquipmentByNameAsync(userId, name, cancellationToken);
        return ToEquipmentDTO(userEquipment);
    }

    [HttpGet("user-equipment/by-name/{name}/details")]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetCurrentUserEquipmentDetailsByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipmentWithDetails = await equipmentService.GetUserEquipmentByNameWithDetailsAsync(userId, name, cancellationToken);
        return ToEquipmentDetailsDTO(userEquipmentWithDetails);
    }


    [HttpPost("user-equipment")]
    public async Task<IActionResult> AddCurrentUserEquipmentAsync([FromForm] EquipmentCreationDTO equipmentCreationDTO, CancellationToken cancellationToken)
    {
        if (equipmentCreationDTO is null)
            return EquipmentIsNull();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = mapper.Map<Equipment>(equipmentCreationDTO);
        equipment = await equipmentService.AddUserEquipmentAsync(userId, equipment, cancellationToken);

        var equipmentDTO = mapper.Map<EquipmentDTO>(equipment);
        return CreatedAtAction(nameof(GetCurrentUserEquipmentByIdAsync), new { equipmentId = equipment.Id }, equipmentDTO);
    }

    [HttpPut("user-equipment/{equipmentId}")]
    public async Task<IActionResult> UpdateCurrentUserEquipmentAsync(long equipmentId, [FromForm] EquipmentUpdateDTO equipmentUpdateDTO, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();


        if (equipmentUpdateDTO is null)
            return EquipmentIsNull();

        if (!AreIdsEqual(equipmentId, equipmentUpdateDTO.Id))
            return EquipmentIDsNotMatch();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = mapper.Map<Equipment>(equipmentUpdateDTO);
        await equipmentService.UpdateUserEquipmentAsync(userId, equipment, cancellationToken);

        return Ok();
    }


    [HttpDelete("user-equipment/{equipmentId}")]
    public async Task<IActionResult> DeleteEquipmentFromCurrentUserAsync(long equipmentId, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        await equipmentService.DeleteEquipmentFromUserAsync(userId, equipmentId, cancellationToken);
        return Ok();
    }

    [HttpPut("user-equipment-photo/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateUserEquipmentPhotoAsync(long equipmentId, [FromForm] FileUploadModel? fileUpload, CancellationToken cancellationToken)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        await equipmentService.UpdateUserEquipmentPhotoAsync(userId, equipmentId, fileUpload, cancellationToken);
        return Ok();
    }

    [HttpDelete("user-equipment-photo/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteUserEquipmentPhotoAsync(long equipmentId, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        await equipmentService.DeleteUserEquipmentPhotoAsync(userId, equipmentId, cancellationToken);
        return Ok();
    }


    #endregion

    #region All Equipments

    [HttpGet("all-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetAllEquipmentsAsync(CancellationToken cancellationToken,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var equipments = await equipmentService.GetAllEquipmentsAsync(userId, cancellationToken);

        var equipmentDTOs = equipments.Select(mapper.Map<EquipmentDTO>);
        return await ApiResult<EquipmentDTO>.CreateAsync(
            equipmentDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("used-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetUsedEquipmentsAsync(CancellationToken cancellationToken,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var usedEquipments = await equipmentService.GetUsedEquipmentsAsync(userId, cancellationToken);

        var equipmentDTOs = usedEquipments.Select(mapper.Map<EquipmentDTO>);
        return await ApiResult<EquipmentDTO>.CreateAsync(
            equipmentDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{equipmentId}/exercises")]
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetExercisesByEquipmentIdAsync(CancellationToken cancellationToken,
        [FromQuery] int equipmentId,
        [FromQuery] int pageIndex = 0,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortColumn = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] string? filterColumn = null,
        [FromQuery] string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = await equipmentService.GetEquipmentByIdAsync(userId, equipmentId, cancellationToken);

        if (equipment is null)
            return EntryNotFound("Equipment");

        if (equipment.Exercises is not IEnumerable<Exercise> exercises)
            return EntryNotFound("Exercises");

        var exerciseDTOs = exercises.ToList().Select(mapper.Map<ExerciseDTO>);
        return await ApiResult<ExerciseDTO>.CreateAsync(
            exerciseDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("equipment/{equipmentId}")]
    [ActionName(nameof(GetEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDTO>> GetEquipmentByIdAsync(long equipmentId, CancellationToken cancellationToken)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = await equipmentService.GetEquipmentByIdAsync(userId, equipmentId, cancellationToken);
        return ToEquipmentDTO(equipment);
    }

    [HttpGet("equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetEquipmentByNameAsync(string name, CancellationToken cancellationToken)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = await equipmentService.GetEquipmentByNameAsync(userId, name, cancellationToken);
        return ToEquipmentDTO(equipment);
    }

    #endregion


    const string equipmentNotFoundStr = "Equipment not found.";
    ActionResult<EquipmentDTO> ToEquipmentDTO(Equipment? equipment)
        => ToDTO<Equipment, EquipmentDTO>(equipment, equipmentNotFoundStr);
    ActionResult<EquipmentDetailsDTO> ToEquipmentDetailsDTO(Equipment? equipment)
        => ToDTO<Equipment, EquipmentDetailsDTO>(equipment, equipmentNotFoundStr);

    ActionResult InvalidEquipmentID()
        => InvalidEntryID(nameof(Equipment));
    ActionResult EquipmentNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Equipment));
    ActionResult EquipmentIsNull()
        => EntryIsNull(nameof(Equipment));
    ActionResult EquipmentIDsNotMatch()
        => EntryIDsNotMatch(nameof(Equipment));
}
