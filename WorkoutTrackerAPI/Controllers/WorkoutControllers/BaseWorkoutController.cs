using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Services.WorkoutServices;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

public abstract class BaseWorkoutController<T> : BaseController<T>
    where T : WorkoutModel
{
    protected ActionResult EntryNameIsNullOrEmpty(string entryName)
        => BadRequest($"{entryName} name is null or empty.");
}