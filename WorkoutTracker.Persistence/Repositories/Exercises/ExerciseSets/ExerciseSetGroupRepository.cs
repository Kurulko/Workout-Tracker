using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseSets;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Exercises.ExerciseSets;

internal class ExerciseSetGroupRepository : DbModelRepository<ExerciseSetGroup>, IExerciseSetGroupRepository
{
    public ExerciseSetGroupRepository(WorkoutDbContext db) : base(db)
    {

    }
}
