using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using WorkoutTracker.API.Controllers.Base;
using WorkoutTracker.API.Results;
using WorkoutTracker.Application.DTOs.Roles;
using WorkoutTracker.Domain.Constants;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Services;

namespace WorkoutTracker.API.Controllers.Auth;

[Authorize(Roles = Roles.AdminRole)]
public class RolesController : APIController
{
    readonly IRoleService roleService;
    readonly IMapper mapper;
    public RolesController(IRoleService roleService, IMapper mapper)
    {
        this.roleService = roleService;
        this.mapper = mapper;
    }


    [HttpGet]
    public async Task<ActionResult<ApiResult<RoleDTO>>> GetRolesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (!IsValidPageIndexAndPageSize(pageIndex, pageSize))
            return InvalidPageIndexOrPageSize();

        var roles = await roleService.GetRolesAsync();

        var roleDTOs = roles.Select(mapper.Map<RoleDTO>);
        return await ApiResult<RoleDTO>.CreateAsync(
            roleDTOs.AsQueryable(),
            pageIndex,
            pageSize,
            sortColumn,
            sortOrder,
            filterColumn,
            filterQuery
        );
    }

    [HttpGet("{roleId}")]
    [ActionName(nameof(GetRoleByIdAsync))]
    public async Task<ActionResult<RoleDTO>> GetRoleByIdAsync(string roleId)
    {
        if (!IsValidID(roleId))
            return RoleIDIsNullOrEmpty();

        var role = await roleService.GetRoleByIdAsync(roleId);
        return ToRoleDTO(role);
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<RoleDTO>> GetRoleByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return RoleNameIsNullOrEmpty();

        var role = await roleService.GetRoleByNameAsync(name);
        return ToRoleDTO(role);
    }

    [HttpPost]
    public async Task<ActionResult<RoleDTO>> AddRoleAsync(RoleCreationDTO roleCreationDTO)
    {
        if (roleCreationDTO is null)
            return RoleIsNull();

        var role = mapper.Map<IdentityRole>(roleCreationDTO);
        role = await roleService.AddRoleAsync(role);

        var roleDTO = mapper.Map<RoleDTO>(role);
        return CreatedAtAction(nameof(GetRoleByIdAsync), new { roleId = roleDTO.Id }, roleDTO);
    }

    [HttpPut("{roleId}")]
    public async Task<IActionResult> UpdateRoleAsync(string roleId, RoleDTO roleDTO)
    {
        if (!IsValidID(roleId))
            return RoleIDIsNullOrEmpty();

        if (roleDTO is null)
            return RoleIsNull();

        if (!AreIdsEqual(roleId, roleDTO.Id))
            return EntryIDsNotMatch("Role");

        var role = mapper.Map<IdentityRole>(roleDTO);
        await roleService.UpdateRoleAsync(role);
        return Ok();
    }

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRoleAsync(string roleId)
    {
        if (!IsValidID(roleId))
            return RoleIDIsNullOrEmpty();

        await roleService.DeleteRoleAsync(roleId);
        return Ok();
    }


    [HttpGet("id-by-name/{name}")]
    public async Task<ActionResult<string>> GetRoleIdByNameAsync(string name)
    {
        if (!IsNameValid(name))
            return RoleNameIsNullOrEmpty();

        string? roleId = await roleService.GetRoleIdByNameAsync(name);

        if (roleId is null)
            return RoleNotFound();

        return roleId;
    }


    [HttpGet("name-by-id/{roleId}")]
    public async Task<ActionResult<string>> GetRoleNameByIdAsync(string roleId)
    {
        if (!IsValidID(roleId))
            return RoleIDIsNullOrEmpty();

        string? name = await roleService.GetRoleNameByIdAsync(roleId);

        if (name is null)
            return RoleNotFound();

        return name;
    }


    static bool IsValidID(string id) => !string.IsNullOrEmpty(id);
    static bool IsNameValid(string name) => !string.IsNullOrEmpty(name);
    static bool AreIdsEqual(string id1, string id2) => id1 == id2;

    ActionResult RoleIDIsNullOrEmpty()
        => BadRequest("Role ID is null or empty.");
    ActionResult RoleNameIsNullOrEmpty()
        => BadRequest("Role name is null or empty.");
    ActionResult RoleIsNull()
        => EntryIsNull("Role");
    ActionResult RoleNotFound()
        => EntryNotFound("Role");

    ActionResult<RoleDTO> ToRoleDTO(IdentityRole? role)
    {
        if (role is null)
            return NotFound("Role not found.");

        var roleDTO = mapper.Map<RoleDTO>(role);
        return roleDTO;
    }
}
