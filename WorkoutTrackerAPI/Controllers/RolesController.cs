using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Services.RoleServices;
using WorkoutTrackerAPI.Data.Models.UserModels;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Services;
using WorkoutTrackerAPI.Services.UserServices;
using Microsoft.IdentityModel.Tokens;
using System.Data;

namespace WorkoutTrackerAPI.Controllers;

[Authorize(Roles = Roles.AdminRole)]
public class RolesController : APIController
{
    readonly IRoleService roleService;
    public RolesController(IRoleService roleService)
        => this.roleService = roleService;

    ActionResult RoleIDIsNullOrEmpty()
        => BadRequest($"Role ID is null or empty.");
    ActionResult RoleNameIsNullOrEmpty()
        => BadRequest($"Role name is null or empty.");
    ActionResult RoleIsNull()
        => EntryIsNull("Role");
    ActionResult RoleNotFound()
        => EntryNotFound("Role");


    [HttpGet]
    public async Task<ActionResult<ApiResult<IdentityRole>>> GetRolesAsync(
        int pageIndex = 0,
        int pageSize = 10,
        string? sortColumn = null,
        string? sortOrder = null,
        string? filterColumn = null,
        string? filterQuery = null)
    {
        if (pageIndex < 0 || pageSize <= 0)
            return InvalidPageIndexOrPageSize();

        try
        {
            var roles = await roleService.GetRolesAsync();

            if (roles is null)
                return EntryNotFound("Roles");

            return await ApiResult<IdentityRole>.CreateAsync(
                roles,
                pageIndex,
                pageSize,
                sortColumn,
                sortOrder,
                filterColumn,
                filterQuery
            );
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("{roleId}")]
    public async Task<ActionResult<IdentityRole>> GetRoleByIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            return RoleIDIsNullOrEmpty();

        try
        {
            IdentityRole? role = await roleService.GetRoleByIdAsync(roleId);

            if (role is null)
                return RoleNotFound();

            return role;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("by-name/{name}")]
    public async Task<ActionResult<IdentityRole>> GetRoleByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return RoleNameIsNullOrEmpty();

        try
        {
            IdentityRole? role = await roleService.GetRoleByNameAsync(name);

            if (role is null)
                return RoleNotFound();

            return role;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPost]
    public async Task<ActionResult<IdentityRole>> AddRoleAsync(IdentityRole role)
    {
        if (role is null)
            return RoleIsNull();

        if (!string.IsNullOrEmpty(role.Id))
            return InvalidEntryIDWhileAdding("Role", "role");

        try
        {
            role = await roleService.AddRoleAsync(role);
            return CreatedAtAction(nameof(GetRoleByIdAsync), new { id = role.Id }, role);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpPut("{roleId}")]
    public async Task<IActionResult> UpdateRoleAsync(string roleId, IdentityRole role)
    {
        if (string.IsNullOrEmpty(roleId))
            return RoleIDIsNullOrEmpty();

        if (role is null)
            return RoleIsNull();

        if (roleId != role.Id)
            return EntryIDsNotMatch("Role");

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

        try
        {
            string? roleId = await roleService.GetRoleIdByNameAsync(name);

            if (roleId is null)
                return NotFound();

            return roleId;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }


    [HttpGet("name-by-id/{roleId}")]
    public async Task<ActionResult<string>> GetRoleNameByIdAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            return RoleIDIsNullOrEmpty();

        try
        {
            string? name = await roleService.GetRoleNameByIdAsync(roleId);

            if (name is null)
                return NotFound();

            return name;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("role-exists/{roleId}")]
    public async Task<ActionResult<bool>> RoleExistsAsync(string roleId)
    {
        if (string.IsNullOrEmpty(roleId))
            return RoleIDIsNullOrEmpty();

        try
        {
            return await roleService.RoleExistsAsync(roleId);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    [HttpGet("role-exists-by-name/{name}")]
    public async Task<ActionResult<bool>> RoleExistsByNameAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            return RoleNameIsNullOrEmpty();

        try
        {
            return await roleService.RoleExistsByNameAsync(name);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
