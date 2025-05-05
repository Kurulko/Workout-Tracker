using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace WorkoutTracker.API.Controllers.Base;

public abstract class BaseWorkoutController<TDTO, TCreationDTO> : DbModelController<TDTO, TCreationDTO>
    where TDTO : class
    where TCreationDTO : class
{
    public BaseWorkoutController(IMapper mapper) : base(mapper)
    {
        
    }

    protected ActionResult EntryNameIsNullOrEmpty(string entryName)
        => BadRequest($"{entryName} name is null or empty.");
}