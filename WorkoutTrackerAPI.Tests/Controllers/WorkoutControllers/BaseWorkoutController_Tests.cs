using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Controllers.WorkoutControllers;
using WorkoutTracker.API.Data.Models;

namespace WorkoutTracker.API.Tests.Controllers.WorkoutControllers;

public class BaseWorkoutController_Tests<T> : DbModelController_Tests<T>
    where T : WorkoutModel
{
}
