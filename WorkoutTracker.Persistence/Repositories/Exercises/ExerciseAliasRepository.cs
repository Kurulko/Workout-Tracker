using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Common.Validators;

namespace WorkoutTracker.Persistence.Repositories.Exercises;

internal class ExerciseAliasRepository : BaseWorkoutRepository<ExerciseAlias>, IExerciseAliasRepository
{
    public ExerciseAliasRepository(WorkoutDbContext db) : base(db)
    {

    }

    public IQueryable<ExerciseAlias> GetExerciseAliasesByExerciseId(long exerciseId)
    {
        ArgumentValidator.ThrowIfIdNonPositive(exerciseId, nameof(Exercise));

        return Find(er => er.ExerciseId == exerciseId);
    }
}
