using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Muscles;

internal class MuscleSizeRepository : DbModelRepository<MuscleSize>, IMuscleSizeRepository
{
    public MuscleSizeRepository(WorkoutDbContext db) : base(db)
    {

    }

    IQueryable<MuscleSize> GetMuscleSizes()
        => dbSet.Include(m => m.Muscle);

    public override Task<IQueryable<MuscleSize>> GetAllAsync()
        => Task.FromResult(GetMuscleSizes());
}