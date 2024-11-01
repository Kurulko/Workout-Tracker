using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

[Authorize]
public abstract class DbModelController<TDTO, TCreationDTO> : APIController
    where TDTO : class
    where TCreationDTO : class
{
    protected readonly IMapper mapper;
    public DbModelController(IMapper mapper)
        => this.mapper = mapper;

    protected ActionResult<TDTO> HandleDTOServiceResult<T>(ServiceResult<T> serviceResult, string? notFoundMessage = null)
        where T : class, IDbModel 
    {
        ServiceResult<TDTO> serviceResultDTO;
        if (serviceResult.Success)
        {
            var modelDTO = mapper.Map<TDTO>(serviceResult.Model);
            serviceResultDTO = ServiceResult<TDTO>.Ok(modelDTO);
        }
        else
        {
            serviceResultDTO = ServiceResult<TDTO>.Fail(serviceResult.ErrorMessage!);
        }

        return HandleServiceResult(serviceResultDTO, notFoundMessage);
    }
}
