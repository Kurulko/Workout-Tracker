using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Extensions;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.Common.Results;
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
    public EquipmentsController(IEquipmentService equipmentService, IFileService fileService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        : base(mapper)
    {
        this.equipmentService = equipmentService;
        this.fileService = fileService;
        this.httpContextAccessor = httpContextAccessor;
    }


    const string equipmentNotFoundStr = "Equipment not found.";
    ActionResult<EquipmentDTO> HandleEquipmentDTOServiceResult(ServiceResult<Equipment> serviceResult)
        => HandleDTOServiceResult<Equipment, EquipmentDTO>(serviceResult, equipmentNotFoundStr);
    ActionResult<EquipmentDetailsDTO> HandleEquipmentDetailsDTOServiceResult(ServiceResult<Equipment> serviceResult)
        => HandleDTOServiceResult<Equipment, EquipmentDetailsDTO>(serviceResult, equipmentNotFoundStr);

    const string userEquipmentNotFoundStr = "User equipment not found.";
    ActionResult<EquipmentDTO> HandleUserEquipmentDTOServiceResult(ServiceResult<Equipment> serviceResult)
        => HandleDTOServiceResult<Equipment, EquipmentDTO>(serviceResult, userEquipmentNotFoundStr);
    ActionResult<EquipmentDetailsDTO> HandleUserEquipmentDetailsDTOServiceResult(ServiceResult<Equipment> serviceResult)
        => HandleDTOServiceResult<Equipment, EquipmentDetailsDTO>(serviceResult, userEquipmentNotFoundStr);

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

        var equipmentDTOs = equipments.ToList().Select(m => mapper.Map<EquipmentDTO>(m));
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

        var equipmentDTOs = equipments.ToList().Select(m => mapper.Map<EquipmentDTO>(m));
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
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetUsedEquipmentsAsync(userId);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not IQueryable<Equipment> equipments)
            return EntryNotFound("Equipments");

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
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetEquipmentByIdAsync(userId, equipmentId, true);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        if (serviceResult.Model is not Equipment equipment)
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


    [HttpGet("internal-equipment/{equipmentId}")]
    [ActionName(nameof(GetInternalEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDTO>> GetInternalEquipmentByIdAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        var serviceResult = await equipmentService.GetInternalEquipmentByIdAsync(equipmentId);
        return HandleEquipmentDTOServiceResult(serviceResult);
    }

    [HttpGet("internal-equipment/{equipmentId}/details")]
    [ActionName(nameof(GetInternalEquipmentDetailsByIdAsync))]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetInternalEquipmentDetailsByIdAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        var serviceResult = await equipmentService.GetInternalEquipmentByIdAsync(equipmentId, true);
        return HandleEquipmentDetailsDTOServiceResult(serviceResult);
    }


    [HttpGet("user-equipment/{equipmentId}")]
    [ActionName(nameof(GetCurrentUserEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDTO>> GetCurrentUserEquipmentByIdAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetUserEquipmentByIdAsync(userId, equipmentId);
        return HandleUserEquipmentDTOServiceResult(serviceResult);
    }

    [HttpGet("user-equipment/{equipmentId}/details")]
    [ActionName(nameof(GetCurrentUserEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetCurrentUserEquipmentDetailsByIdAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetUserEquipmentByIdAsync(userId, equipmentId, true);
        return HandleUserEquipmentDetailsDTOServiceResult(serviceResult);
    }


    [HttpGet("internal-equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetInternalEquipmentByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        var serviceResult = await equipmentService.GetInternalEquipmentByNameAsync(name);
        return HandleEquipmentDTOServiceResult(serviceResult);
    }

    [HttpGet("internal-equipment/by-name/{name}/details")]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetInternalEquipmentDetailsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        var serviceResult = await equipmentService.GetInternalEquipmentByNameAsync(name, true);
        return HandleEquipmentDetailsDTOServiceResult(serviceResult);
    }


    [HttpGet("user-equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetCurrentUserEquipmentByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetUserEquipmentByNameAsync(userId, name);
        return HandleUserEquipmentDTOServiceResult(serviceResult);
    }

    [HttpGet("user-equipment/by-name/{name}/details")]
    public async Task<ActionResult<EquipmentDetailsDTO>> GetCurrentUserEquipmentDetailsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetUserEquipmentByNameAsync(userId, name, true);
        return HandleUserEquipmentDetailsDTOServiceResult(serviceResult);
    }

    [HttpGet("equipment/{equipmentId}")]
    [ActionName(nameof(GetEquipmentByIdAsync))]
    public async Task<ActionResult<EquipmentDTO>> GetEquipmentByIdAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetEquipmentByIdAsync(userId, equipmentId);
        return HandleUserEquipmentDTOServiceResult(serviceResult);
    }

    [HttpGet("equipment/by-name/{name}")]
    public async Task<ActionResult<EquipmentDTO>> GetEquipmentByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.GetEquipmentByNameAsync(userId, name);
        return HandleEquipmentDTOServiceResult(serviceResult);
    }


    readonly string equipmentPhotosDirectory = Path.Combine("photos", "equipments");
    const int maxEquipmentImageSizeInMB = 3;

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

        var serviceResult = await equipmentService.AddInternalEquipmentAsync(equipment);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        equipment = serviceResult.Model!;

        var equipmentDTO = mapper.Map<EquipmentDTO>(equipment);
        return CreatedAtAction(nameof(GetInternalEquipmentByIdAsync), new { equipmentId = equipment.Id }, equipmentDTO);
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
        var serviceResult = await equipmentService.AddUserEquipmentAsync(userId, equipment);

        if (!serviceResult.Success)
            return BadRequest(serviceResult.ErrorMessage);

        equipment = serviceResult.Model!;
        var equipmentDTO = mapper.Map<EquipmentDTO>(equipment);

        return CreatedAtAction(nameof(GetCurrentUserEquipmentByIdAsync), new { equipmentId = equipment.Id }, equipmentDTO);
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

        if (equipmentId != equipmentUpdateDTO.Id)
            return EquipmentIDsNotMatch();

        string? image = await fileService.GetImage(imageFile, equipmentPhotosDirectory, maxEquipmentImageSizeInMB, false);
        var equipment = mapper.Map<Equipment>(equipmentUpdateDTO);
        equipment.Image = image ?? equipmentUpdateDTO.Image;

        var serviceResult = await equipmentService.UpdateInternalEquipmentAsync(equipment);

        if (serviceResult.Success && imageFile != null && equipmentUpdateDTO.Image is string oldImage)
        {
            fileService.DeleteFile(oldImage);
        }

        return HandleServiceResult(serviceResult);
    }

    [HttpPut("user-equipment/{equipmentId}")]
    public async Task<IActionResult> UpdateCurrentUserEquipmentAsync(long equipmentId, [FromForm] UploadWithPhoto<EquipmentUpdateDTO> equipmentUpdateDTOWithPhoto)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        var (equipmentUpdateDTO, imageFile) = (equipmentUpdateDTOWithPhoto.Model, equipmentUpdateDTOWithPhoto.Photo);

        if (equipmentUpdateDTO is null)
            return EquipmentIsNull();

        if (equipmentId != equipmentUpdateDTO.Id)
            return EquipmentIDsNotMatch();

        string? image = await fileService.GetImage(imageFile, equipmentPhotosDirectory, maxEquipmentImageSizeInMB);
        var equipment = mapper.Map<Equipment>(equipmentUpdateDTO);
        equipment.Image = image ?? equipmentUpdateDTO.Image;

        string userId = httpContextAccessor.GetUserId()!;
        var serviceResult = await equipmentService.UpdateUserEquipmentAsync(userId, equipment);

        if (serviceResult.Success && imageFile != null && equipmentUpdateDTO.Image is string oldImage)
        {
            fileService.DeleteFile(oldImage);
        }

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

        return await equipmentService.InternalEquipmentExistsAsync(equipmentId);
    }

    [HttpGet("user-equipment-exists/{equipmentId}")]
    public async Task<ActionResult<bool>> CurrentUserEquipmentExistsAsync(long equipmentId)
    {
        if (equipmentId < 1)
            return InvalidEquipmentID();

        string userId = httpContextAccessor.GetUserId()!;
        return await equipmentService.UserEquipmentExistsAsync(userId, equipmentId);
    }

    [HttpGet("internal-equipment-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> InternalEquipmentExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        return await equipmentService.InternalEquipmentExistsByNameAsync(name);
    }

    [HttpGet("user-equipment-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> CurrentUserEquipmentExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return EquipmentNameIsNullOrEmpty();

        string userId = httpContextAccessor.GetUserId()!;
        return await equipmentService.UserEquipmentExistsByNameAsync(userId, name);
    }
}
