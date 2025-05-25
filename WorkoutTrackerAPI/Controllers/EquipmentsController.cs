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

namespace WorkoutTracker.API.Controllers;

public class EquipmentsController : BaseWorkoutController<EquipmentDTO, EquipmentDTO>
{
    readonly IEquipmentService equipmentService;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly IFileService fileService;
    public EquipmentsController (
        IEquipmentService equipmentService, 
        IHttpContextAccessor httpContextAccessor,
        IFileService fileService, 
        IMapper mapper
    ) : base(mapper)
    {
        this.equipmentService = equipmentService;
        this.fileService = fileService;
        this.httpContextAccessor = httpContextAccessor;
    }

    readonly string equipmentPhotosDirectory = Path.Combine("photos", "equipments");
    const int maxEquipmentImageSizeInMB = 3;


    #region Internal Equipments

    [HttpGet("internal-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetInternalEquipmentsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var equipments = await equipmentService.GetInternalEquipmentsAsync();

        var equipmentDTOs = equipments.ToList().Select(mapper.Map<EquipmentDTO>);
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
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        var internalEquipment = await equipmentService.GetInternalEquipmentByIdAsync(equipmentId);
        return ToEquipmentDTO(internalEquipment);
    }

    [HttpGet("internal-equipment/{equipmentId}/details")]
    [ActionName(nameof(GetInternalEquipmentDetailsByIdAsync))]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetInternalEquipmentDetailsByIdAsync(long equipmentId)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        var internalEquipmentWithDetails = await equipmentService.GetInternalEquipmentByIdAsync(equipmentId, true);
        return ToEquipmentDetailsDTO(internalEquipmentWithDetails);
    }

    [HttpGet("internal-equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetInternalEquipmentByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        var userEquipment = await equipmentService.GetInternalEquipmentByNameAsync(name);
        return ToEquipmentDTO(userEquipment);
    }

    [HttpGet("internal-equipment/by-name/{name}/details")]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetInternalEquipmentDetailsByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        var userEquipmentDetails = await equipmentService.GetInternalEquipmentByNameAsync(name, true);
        return ToEquipmentDetailsDTO(userEquipmentDetails);
    }

    [HttpPost("internal-equipment")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> AddInternalEquipmentAsync([FromForm] UploadWithPhoto<EquipmentCreationDTO> equipmentCreationDTOWithPhoto)
    {
        var (equipmentCreationDTO, imageFile) = (equipmentCreationDTOWithPhoto.Model, equipmentCreationDTOWithPhoto.Photo);

        if (equipmentCreationDTO is null)
            return EquipmentIsNull();

        string? image = await fileService.GetImage(imageFile, equipmentPhotosDirectory, maxEquipmentImageSizeInMB, false);
        var equipment = mapper.Map<Equipment>(equipmentCreationDTO);
        equipment.Image = image;

        equipment = await equipmentService.AddInternalEquipmentAsync(equipment);

        var equipmentDTO = mapper.Map<EquipmentDTO>(equipment);
        return CreatedAtAction(nameof(GetInternalEquipmentByIdAsync), new { equipmentId = equipment.Id }, equipmentDTO);
    }

    [HttpPut("internal-equipment/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> UpdateInternalEquipmentAsync(long equipmentId, [FromForm] UploadWithPhoto<EquipmentUpdateDTO> equipmentUpdateDTOWithPhoto)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        var (equipmentUpdateDTO, imageFile) = (equipmentUpdateDTOWithPhoto.Model, equipmentUpdateDTOWithPhoto.Photo);

        if (equipmentUpdateDTO is null)
            return EquipmentIsNull();

        if (!AreIdsEqual(equipmentId, equipmentUpdateDTO.Id))
            return EquipmentIDsNotMatch();

        string? image = await fileService.GetImage(imageFile, equipmentPhotosDirectory, maxEquipmentImageSizeInMB, false);
        var equipment = mapper.Map<Equipment>(equipmentUpdateDTO);
        equipment.Image = image ?? equipmentUpdateDTO.Image;

        await equipmentService.UpdateInternalEquipmentAsync(equipment);

        if (imageFile != null && equipmentUpdateDTO.Image is string oldImage)
            fileService.DeleteFile(oldImage);

        return Ok();
    }

    [HttpDelete("internal-equipment/{equipmentId}")]
    [Authorize(Roles = Roles.AdminRole)]
    public async Task<IActionResult> DeleteInternalEquipmentAsync(long equipmentId)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        await equipmentService.DeleteInternalEquipmentAsync(equipmentId);
        return Ok();
    }

    #endregion

    #region User Equipments

    [HttpGet("user-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetCurrentUserEquipmentsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipments = await equipmentService.GetUserEquipmentsAsync(userId);

        var equipmentDTOs = userEquipments.ToList().Select(m => mapper.Map<EquipmentDTO>(m));
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
    public async Task<ActionResult<EquipmentDTO>> GetCurrentUserEquipmentByIdAsync(long equipmentId)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipment = await equipmentService.GetUserEquipmentByIdAsync(userId, equipmentId);
        return ToEquipmentDTO(userEquipment);
    }

    [HttpGet("user-equipment/{equipmentId}/details")]
    [ActionName(nameof(GetCurrentUserEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetCurrentUserEquipmentDetailsByIdAsync(long equipmentId)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipmentWithDetails = await equipmentService.GetUserEquipmentByIdAsync(userId, equipmentId, true);
        return ToEquipmentDetailsDTO(userEquipmentWithDetails);
    }


    [HttpGet("user-equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetCurrentUserEquipmentByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipment = await equipmentService.GetUserEquipmentByNameAsync(userId, name);
        return ToEquipmentDTO(userEquipment);
    }

    [HttpGet("user-equipment/by-name/{name}/details")]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetCurrentUserEquipmentDetailsByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var userEquipmentWithDetails = await equipmentService.GetUserEquipmentByNameAsync(userId, name, true);
        return ToEquipmentDetailsDTO(userEquipmentWithDetails);
    }


    [HttpPost("user-equipment")]
    public async Task<IActionResult> AddCurrentUserEquipmentAsync([FromForm] UploadWithPhoto<EquipmentCreationDTO> equipmentCreationDTOWithPhoto)
    {
        var (equipmentCreationDTO, imageFile) = (equipmentCreationDTOWithPhoto.Model, equipmentCreationDTOWithPhoto.Photo);

        if (equipmentCreationDTO is null)
            return EquipmentIsNull();

        string? image = await fileService.GetImage(imageFile, equipmentPhotosDirectory, maxEquipmentImageSizeInMB);
        var equipment = mapper.Map<Equipment>(equipmentCreationDTO);
        equipment.Image = image;

        string userId = httpContextAccessor.GetUserId()!;
        equipment = await equipmentService.AddUserEquipmentAsync(userId, equipment);

        var equipmentDTO = mapper.Map<EquipmentDTO>(equipment);
        return CreatedAtAction(nameof(GetCurrentUserEquipmentByIdAsync), new { equipmentId = equipment.Id }, equipmentDTO);
    }

    [HttpPut("user-equipment/{equipmentId}")]
    public async Task<IActionResult> UpdateCurrentUserEquipmentAsync(long equipmentId, [FromForm] UploadWithPhoto<EquipmentUpdateDTO> equipmentUpdateDTOWithPhoto)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        var (equipmentUpdateDTO, imageFile) = (equipmentUpdateDTOWithPhoto.Model, equipmentUpdateDTOWithPhoto.Photo);

        if (equipmentUpdateDTO is null)
            return EquipmentIsNull();

        if (!AreIdsEqual(equipmentId, equipmentUpdateDTO.Id))
            return EquipmentIDsNotMatch();

        string? image = await fileService.GetImage(imageFile, equipmentPhotosDirectory, maxEquipmentImageSizeInMB);
        var equipment = mapper.Map<Equipment>(equipmentUpdateDTO);
        equipment.Image = image ?? equipmentUpdateDTO.Image;

        string userId = httpContextAccessor.GetUserId()!;
        await equipmentService.UpdateUserEquipmentAsync(userId, equipment);

        if (imageFile != null && equipmentUpdateDTO.Image is string oldImage)
            fileService.DeleteFile(oldImage);

        return Ok();
    }


    [HttpDelete("user-equipment/{equipmentId}")]
    public async Task<IActionResult> DeleteEquipmentFromCurrentUserAsync(long equipmentId)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        await equipmentService.DeleteEquipmentFromUserAsync(userId, equipmentId);
        return Ok();
    }

    #endregion

    #region All Equipments

    [HttpGet("all-equipments")]
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetAllEquipmentsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var equipments = await equipmentService.GetAllEquipmentsAsync(userId);

        var equipmentDTOs = equipments.ToList().Select(mapper.Map<EquipmentDTO>);
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
    public async Task<ActionResult<ApiResult<EquipmentDTO>>> GetUsedEquipmentsAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var usedEquipments = await equipmentService.GetUsedEquipmentsAsync(userId);

        var equipmentDTOs = usedEquipments.ToList().Select(mapper.Map<EquipmentDTO>);
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
    public async Task<ActionResult<ApiResult<ExerciseDTO>>> GetExercisesByEquipmentIdAsync(
        int equipmentId,
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = await equipmentService.GetEquipmentByIdAsync(userId, equipmentId, true);

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
    public async Task<ActionResult<EquipmentDTO>> GetEquipmentByIdAsync(long equipmentId)
    {
        if (!IsValidID(equipmentId))
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = await equipmentService.GetEquipmentByIdAsync(userId, equipmentId);
        return ToEquipmentDTO(equipment);
    }

    [HttpGet("equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetEquipmentByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var equipment = await equipmentService.GetEquipmentByNameAsync(userId, name);
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
