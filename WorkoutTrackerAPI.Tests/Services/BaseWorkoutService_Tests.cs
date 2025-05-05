using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTracker.API.Data.Models;

namespace WorkoutTracker.API.Tests.Services;

public class BaseWorkoutService_Tests<TModel> : DbModelService_Tests<TModel> 
    where TModel : WorkoutModel
{
}
