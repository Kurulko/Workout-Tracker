using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Exercises.ExerciseSets;

internal class ExerciseSetRepository : DbModelRepository<ExerciseSet>, IExerciseSetRepository
{
    public ExerciseSetRepository(WorkoutDbContext db) : base(db)
    {

    }
}

