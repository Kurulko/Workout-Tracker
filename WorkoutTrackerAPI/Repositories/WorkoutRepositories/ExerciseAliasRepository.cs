using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Repositories.WorkoutRepositories;

public class ExerciseAliasRepository : BaseWorkoutRepository<ExerciseAlias>
{
    public ExerciseAliasRepository(WorkoutDbContext db) : base(db)
    {

    }
}
