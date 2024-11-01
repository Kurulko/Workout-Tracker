using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Services.WorkoutServices;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public abstract class BaseWorkoutController<T, TDTO> : DbModelController<T, TDTO>
    where T : WorkoutModel
    where TDTO : WorkoutModel
{
    public BaseWorkoutController(IMapper mapper) : base(mapper)
    {
        
    }

    protected ActionResult EntryNameIsNullOrEmpty(string entryName)
        => BadRequest($"{entryName} name is null or empty.");
}