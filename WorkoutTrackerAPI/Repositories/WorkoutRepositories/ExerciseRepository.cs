using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace WorkoutTrackerAPI.Repositories;

public class ExerciseRepository : BaseWorkoutRepository<Exercise>
{
    public ExerciseRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<Exercise> GetExercises()
        => dbSet.Include(m => m.WorkingMuscles)
        .Include(m => m.Equipments)
        .Include(m => m.ExerciseRecords);

    public override Task<IQueryable<Exercise>> GetAllAsync()
        => Task.FromResult(GetExercises());
}