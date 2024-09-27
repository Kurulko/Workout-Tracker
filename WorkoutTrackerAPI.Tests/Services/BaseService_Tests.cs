using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Repositories;

namespace WorkoutTrackerAPI.Tests.Services;

public abstract class BaseService_Tests<TModel> where TModel : class, IDbModel
{
    protected readonly WorkoutContextFactory factory = new();
}