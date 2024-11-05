using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Services.EquipmentServices;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public class EquipmentsController : BaseWorkoutController<EquipmentDTO, EquipmentDTO>
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

    [HttpGet("internal-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetInternalEquipmentsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var serviceResult = await equipmentService.GetInternalEquipmentsAsync();

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Equipment> equipments)
            return EntryNotFound("Equipments");

        var equipmentDTOs = equipments.AsEnumerable().Select(m => mapper.Map<EquipmentDTO>(m));
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

        var equipmentDTOs = equipments.AsEnumerable().Select(m => mapper.Map<EquipmentDTO>(m));
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

    [HttpGet("all-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetAllEquipmentsAsync(
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
        var serviceResult = await equipmentService.GetAllEquipmentsAsync(userId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Equipment> equipments)
            return EntryNotFound("Equipments");

        var equipmentDTOs = equipments.AsEnumerable().Select(m => mapper.Map<EquipmentDTO>(m));
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
    public async Task<ActionResult<EquipmentDTO>> GetInternalEquipmentByIdAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        var serviceResult = await equipmentService.GetInternalEquipmentByIdAsync(equipmentId);
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

    [HttpGet("internal-equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetInternalEquipmentByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        var serviceResult = await equipmentService.GetInternalEquipmentByNameAsync(name);
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

    [HttpPost("internal-equipment")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddInternalEquipmentAsync(EquipmentDTO equipmentDTO)
    {
        if (equipmentDTO is null)
            return EquipmentIsNull();

        if (equipmentDTO.Id != 0)
            return InvalidEquipmentIDWhileAdding();

        var equipment = mapper.Map<Equipment>(equipmentDTO);
        var serviceResult = await equipmentService.AddInternalEquipmentAsync(equipment);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        equipment = serviceResult.Model!;

        return CreatedAtAction(nameof(GetInternalEquipmentByIdAsync), new { equipmentId = equipment.Id }, equipment);
    }

    [HttpPost("user-equipment")]
    public async Task<IActionResult> AddCurrentUserEquipmentAsync(EquipmentDTO equipmentDTO)
    {
        if (equipmentDTO is null)
            return EquipmentIsNull();

        if (equipmentDTO.Id != 0)
            return InvalidEquipmentIDWhileAdding();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = mapper.Map<Equipment>(equipmentDTO);
        var serviceResult = await equipmentService.AddUserEquipmentAsync(userId, equipment);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        equipment = serviceResult.Model!;

        return CreatedAtAction(nameof(GetCurrentUserEquipmentByIdAsync), new { equipmentId = equipment.Id }, equipment);
    }

    [HttpPut("internal-equipment/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalEquipmentAsync(long equipmentId, EquipmentDTO equipmentDTO)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        if (equipmentDTO is null)
            return EquipmentIsNull();

        if (equipmentId != equipmentDTO.Id)
            return EquipmentIDsNotMatch();

        var equipment = mapper.Map<Equipment>(equipmentDTO);
        var serviceResult = await equipmentService.UpdateInternalEquipmentAsync(equipment);
        return HandleServiceResult(serviceResult);
    }

    [HttpPut("user-equipment/{equipmentId}")]
    public async Task<IActionResult> UpdateCurrentUserEquipmentAsync(long equipmentId, EquipmentDTO equipmentDTO)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        if (equipmentDTO is null)
            return EquipmentIsNull();

        if (equipmentId != equipmentDTO.Id)
            return EquipmentIDsNotMatch();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = mapper.Map<Equipment>(equipmentDTO);
        var serviceResult = await equipmentService.UpdateUserEquipmentAsync(userId, equipment);
        return HandleServiceResult(serviceResult);
    }

    [HttpDelete("internal-equipment/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteInternalEquipmentAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        var serviceResult = await equipmentService.DeleteInternalEquipmentAsync(equipmentId);
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

    [HttpGet("internal-equipment-exists/{equipmentId}")]
    public async Task<ActionResult<bool>> InternalEquipmentExistsAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        try
        {
            return await equipmentService.InternalEquipmentExistsAsync(equipmentId);
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

    [HttpGet("internal-equipment-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> InternalEquipmentExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        try
        {
            return await equipmentService.InternalEquipmentExistsByNameAsync(name);
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
