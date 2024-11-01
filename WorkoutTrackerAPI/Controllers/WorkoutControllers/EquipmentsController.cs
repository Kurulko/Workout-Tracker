using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Services.EquipmentServices;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public class EquipmentsController : BaseWorkoutController<Equipment, EquipmentDTO>
{
    readonly IEquipmentService equipmentService;
    readonly IHttpContextAccessor httpContextAccessor;
    public EquipmentsController(IEquipmentService equipmentService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(mapper)
    {
        this.equipmentService = equipmentService;
        this.httpContextAccessor = httpContextAccessor;
    }

    ActionResult<EquipmentDTO> HandleEquipmentDTOServiceResult(ServiceResult<Equipment> serviceResult)
        => HandleDTOServiceResult(serviceResult, "Equipment not found.");

    ActionResult InvalidEquipmentID()
        => InvalidEntryID(nameof(Equipment));
    ActionResult EquipmentNameIsNullOrEmpty()
        => EntryNameIsNullOrEmpty(nameof(Equipment));
    ActionResult EquipmentIsNull()
        => EntryIsNull(nameof(Equipment));
    ActionResult EquipmentIDsNotMatch()
        => EntryIDsNotMatch(nameof(Equipment));
    ActionResult InvalidEquipmentIDWhileAdding()
        => InvalidEntryIDWhileAdding(nameof(Equipment), "equipment");

    [HttpGet("equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetEquipmentsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await equipmentService.GetEquipmentsAsync();

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Equipment> equipments)
            return EntryNotFound("Equipments");

        var equipmentDTOs = equipments.Select(m => mapper.Map<EquipmentDTO>(m));
        return await ApiResult<EquipmentDTO>.CreateAsync(
            equipmentDTOs,
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("user-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetCurrentUserEquipmentsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetUserEquipmentsAsync(userId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Equipment> equipments)
            return EntryNotFound("Equipments");

        var equipmentDTOs = equipments.Select(m => mapper.Map<EquipmentDTO>(m));
        return await ApiResult<EquipmentDTO>.CreateAsync(
            equipmentDTOs,
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
    public async Task<ActionResult<EquipmentDTO>> GetEquipmentByIdAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        var serviceResult = await equipmentService.GetEquipmentByIdAsync(equipmentId);
        return HandleEquipmentDTOServiceResult(serviceResult);
    }

    [HttpGet("user-equipment/{equipmentId}")]
    [ActionName(nameof(GetCurrentUserEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDTO>> GetCurrentUserEquipmentByIdAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetUserEquipmentByIdAsync(userId, equipmentId);
        return HandleDTOServiceResult(serviceResult, "User equipment not found.");
    }

    [HttpGet("equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetEquipmentByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        var serviceResult = await equipmentService.GetEquipmentByNameAsync(name);
        return HandleEquipmentDTOServiceResult(serviceResult);
    }

    [HttpGet("user-equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetCurrentUserEquipmentByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetUserEquipmentByNameAsync(userId, name);
        return HandleDTOServiceResult(serviceResult, "User equipment not found.");
    }

    [HttpPost]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddEquipmentAsync(Equipment equipment)
    {
        if (equipment is null)
            return EquipmentIsNull();

        if (equipment.Id != 0)
            return InvalidEquipmentIDWhileAdding();

        var serviceResult = await equipmentService.AddEquipmentAsync(equipment);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        equipment = serviceResult.Model!;

        return CreatedAtAction(nameof(GetEquipmentByIdAsync), new { id = equipment.Id }, equipment);
    }

    [HttpPost("user-equipment")]
    public async Task<IActionResult> AddCurrentUserEquipmentAsync(Equipment equipment)
    {
        if (equipment is null)
            return EquipmentIsNull();

        if (equipment.Id != 0)
            return InvalidEquipmentIDWhileAdding();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.AddUserEquipmentAsync(userId, equipment);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        equipment = serviceResult.Model!;

        return CreatedAtAction(nameof(GetCurrentUserEquipmentByIdAsync), new { id = equipment.Id }, equipment);
    }

    [HttpPut("{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateEquipmentAsync(long equipmentId, Equipment equipment)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        if (equipment is null)
            return EquipmentIsNull();

        if (equipmentId != equipment.Id)
            return EquipmentIDsNotMatch();

        var serviceResult = await equipmentService.UpdateEquipmentAsync(equipment);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("user-equipment/{equipmentId}")]
    public async Task<IActionResult> UpdateCurrentUserEquipmentAsync(long equipmentId, Equipment equipment)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        if (equipment is null)
            return EquipmentIsNull();

        if (equipmentId != equipment.Id)
            return EquipmentIDsNotMatch();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.UpdateUserEquipmentAsync(userId, equipment);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteEquipmentAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        var serviceResult = await equipmentService.DeleteEquipmentAsync(equipmentId);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("user-equipment/{equipmentId}")]
    public async Task<IActionResult> DeleteEquipmentFromCurrentUserAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.DeleteEquipmentFromUserAsync(userId, equipmentId);
        return HandleServiceResult(serviceResult);
    }

    [HttpGet("equipment-exists/{equipmentId}")]
    public async Task<ActionResult<bool>> EquipmentExistsAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        try
        {
            return await equipmentService.EquipmentExistsAsync(equipmentId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("user-equipment-exists/{equipmentId}")]
    public async Task<ActionResult<bool>> CurrentUserEquipmentExistsAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            return await equipmentService.UserEquipmentExistsAsync(userId, equipmentId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("equipment-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> EquipmentExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        try
        {
            return await equipmentService.EquipmentExistsByNameAsync(name);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("user-equipment-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> CurrentUserEquipmentExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            return await equipmentService.UserEquipmentExistsByNameAsync(userId, name);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
