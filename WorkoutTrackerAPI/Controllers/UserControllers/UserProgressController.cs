using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Services.ProgressServices;

namespace WorkoutTrackerAPI.Controllers.UserControllers;

[Authorize]
[Route("api/user-progress")]
public class UserProgressController : APIController
{
    readonly IUserProgressService userProgressService;
    readonly IHttpContextAccessor httpContextAccessor;
    readonly IMapper mapper;
    public UserProgressController(IUserProgressService userProgressService, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        this.userProgressService = userProgressService;
        this.mapper = mapper;
        this.httpContextAccessor = httpContextAccessor;
    }

    [HttpGet]
    public async Task<ActionResult<TotalUserProgressDTO>> CalculateCurrentUserProgressAsync()
    {
        try
        {
            string userId = httpContextAccessor.GetUserId()!;
            var userProgress = await userProgressService.CalculateUserProgressAsync(userId);
            var userProgressDTO = mapper.Map<TotalUserProgressDTO>(userProgress);
            return userProgressDTO;
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }
}
