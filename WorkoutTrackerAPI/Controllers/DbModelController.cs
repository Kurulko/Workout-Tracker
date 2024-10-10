using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Services;

namespace WorkoutTrackerAPI.Controllers.WorkoutControllers;

[Authorize]
public abstract class DbModelController<T> : APIController
    where T : class, IDbModel
{
}
