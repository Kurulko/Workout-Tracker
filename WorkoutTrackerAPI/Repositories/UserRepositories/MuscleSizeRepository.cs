using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Models;
using System.Linq.Expressions;

namespace WorkoutTrackerAPI.Repositories;

public class MuscleSizeRepository : DbModelRepository<MuscleSize>
{
    public MuscleSizeRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<MuscleSize> GetMuscleSizes()
        => dbSet.Include(m => m.Muscle);

    public override Task<IQueryable<MuscleSize>> GetAllAsync()
        => Task.FromResult(GetMuscleSizes());
}