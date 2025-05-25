using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Numerics;
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


    protected bool IsValidID<TNumber>(TNumber id) where TNumber : struct, INumber<TNumber>
        => id > TNumber.Zero;
    protected bool IsValidIDWhileAdding<TNumber>(TNumber id) where TNumber : struct, INumber<TNumber>
        => id == TNumber.Zero;
    protected bool AreIdsEqual<TNumber>(TNumber id1, TNumber id2) where TNumber : struct, INumber<TNumber>
        => id1 == id2;


    protected ActionResult<T_DTO> ToDTO<T, T_DTO>(T? model, string? notFoundMessage = null)
        where T : class, IDbModel
        where T_DTO : class
    {
        if (model is null)
            return NotFound(notFoundMessage);

        var modelDTO = mapper.Map<T_DTO>(model);
        return modelDTO;
    }
}
