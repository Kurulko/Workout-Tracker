using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Tests.Services;

public class BaseWorkoutService_Tests<TModel> : BaseService_Tests<TModel> 
    where TModel : WorkoutModel
{
}
