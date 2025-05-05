using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises.ExerciseRecords;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Persistence.Repositories.Base;

namespace WorkoutTracker.Persistence.Repositories.Exercises.ExerciseRecords;

internal class ExerciseRecordGroupRepository : DbModelRepository<ExerciseRecordGroup>, IExerciseRecordGroupRepository
{
    public ExerciseRecordGroupRepository(WorkoutDbContext db) : base(db)
    {

    }
}
