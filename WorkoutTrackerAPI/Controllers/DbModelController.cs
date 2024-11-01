using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

[Authorize]
public abstract class DbModelController<T, TDTO> : APIController
    where T : class, IDbModel
    where TDTO : class, IDbModel
{
    protected readonly IMapper mapper;
    public DbModelController(IMapper mapper)
        => this.mapper = mapper;

    protected ActionResult<TDTO> HandleDTOServiceResult(ServiceResult<T> serviceResult, string? notFoundMessage = null)
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
