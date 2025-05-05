using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.API.Controllers.Base;

[Authorize]
public abstract class DbModelController<TDTO, TCreationDTO> : APIController
    where TDTO : class
    where TCreationDTO : class
{
    protected readonly IMapper mapper;
    public DbModelController(IMapper mapper)
        => this.mapper = mapper;

    protected ActionResult<T_DTO> HandleDTOServiceResult<T, T_DTO>(ServiceResult<T> serviceResult, string? notFoundMessage = null)
        where T : class, IDbModel 
        where T_DTO : class 
    {
        ServiceResult<T_DTO> serviceResultDTO;
        if (serviceResult.Success)
        {
            var modelDTO = mapper.Map<T_DTO>(serviceResult.Model);
            serviceResultDTO = ServiceResult<T_DTO>.Ok(modelDTO);
        }
        else
        {
            serviceResultDTO = ServiceResult<T_DTO>.Fail(serviceResult.ErrorMessage!);
        }

        return HandleServiceResult(serviceResultDTO, notFoundMessage);
    }
}
