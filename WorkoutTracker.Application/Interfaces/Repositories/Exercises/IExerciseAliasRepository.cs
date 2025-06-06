﻿using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories.Exercises;

public interface IExerciseAliasRepository : IBaseWorkoutRepository<ExerciseAlias>
{
    IQueryable<ExerciseAlias> GetExerciseAliasesByExerciseId(long exerciseId);

    Task RemoveByExerciseIdAsync(long exerciseId, CancellationToken cancellationToken = default);
}
