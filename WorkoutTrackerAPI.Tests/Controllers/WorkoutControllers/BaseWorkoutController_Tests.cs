using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Controllers.WorkoutControllers;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Tests.Controllers.WorkoutControllers;

public class BaseWorkoutController_Tests<T> : DbModelController_Tests<T>
    where T : WorkoutModel
{
}
