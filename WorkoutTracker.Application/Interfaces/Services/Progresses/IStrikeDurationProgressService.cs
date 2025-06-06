﻿using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Models.Progresses;

namespace WorkoutTracker.Application.Interfaces.Services.Progresses;

public interface IStrikeDurationProgressService : IBaseService
{
    StrikeDurationProgress CalculateStrikeDurationProgress(BaseInfoProgress baseInfoProgress, DateTimeRange range);
}
