using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkoutTrackerAPI.Tests;

public abstract class BaseTests
{
    protected readonly WorkoutContextFactory contextFactory = new();
}
