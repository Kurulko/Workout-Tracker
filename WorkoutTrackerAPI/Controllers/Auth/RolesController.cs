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

    ActionResult RoleIDIsNullOrEmpty()
        => BadRequest("Role ID is null or empty.");
    ActionResult RoleNameIsNullOrEmpty()
        => BadRequest("Role name is null or empty.");
    ActionResult RoleIsNull()
        => EntryIsNull("Role");
    ActionResult RoleNotFound()
        => EntryNotFound("Role");


    [HttpGet]
    public async Task<ActionResult<ApiResult<RoleDTO>>> GetRolesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        var roles = await roleService.GetRolesAsync();

        if (roles is null)
            return EntryNotFound("Roles");

        var roleDTOs = roles.ToList().Select(mapper.Map<RoleDTO>);
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
        if (string.IsNullOrEmpty(roleId))
            return RoleIDIsNullOrEmpty();

        IdentityRole? role = await roleService.GetRoleByIdAsync(roleId);

        if (role is null)
            return RoleNotFound();

        return mapper.Map<RoleDTO>(role);
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<RoleDTO>> GetRoleByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return RoleNameIsNullOrEmpty();

        IdentityRole? role = await roleService.GetRoleByNameAsync(name);

        if (role is null)
            return RoleNotFound();

        return mapper.Map<RoleDTO>(role);
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
        if (string.IsNullOrEmpty(roleId))
            return RoleIDIsNullOrEmpty();

        if (roleDTO is null)
            return RoleIsNull();

        if (roleId != roleDTO.Id)
            return EntryIDsNotMatch("Role");

        var role = mapper.Map<IdentityRole>(roleDTO);
        var identityResult = await roleService.UpdateRoleAsync(role);
        return HandleIdentityResult(identityResult);
    }

    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRoleAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            return RoleIDIsNullOrEmpty();

        var identityResult = await roleService.DeleteRoleAsync(roleId);
        return HandleIdentityResult(identityResult);
    }


    [HttpGet("id-by-name/{name}")]
    public async Task<ActionResult<string>> GetRoleIdByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return RoleNameIsNullOrEmpty();

        string? roleId = await roleService.GetRoleIdByNameAsync(name);

        if (roleId is null)
            return RoleNotFound();

        return roleId;
    }


    [HttpGet("name-by-id/{roleId}")]
    public async Task<ActionResult<string>> GetRoleNameByIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            return RoleIDIsNullOrEmpty();

        string? name = await roleService.GetRoleNameByIdAsync(roleId);

        if (name is null)
            return RoleNotFound();

        return name;
    }

    [HttpGet("role-exists/{roleId}")]
    public async Task<ActionResult<bool>> RoleExistsAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            return RoleIDIsNullOrEmpty();

        return await roleService.RoleExistsAsync(roleId);
    }

    [HttpGet("role-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> RoleExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return RoleNameIsNullOrEmpty();

        return await roleService.RoleExistsByNameAsync(name);
    }
}
