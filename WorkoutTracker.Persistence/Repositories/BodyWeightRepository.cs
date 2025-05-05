using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories;

internal class BodyWeightRepository : DbModelRepository<BodyWeight>, IBodyWeightRepository
{
    public BodyWeightRepository(WorkoutDbContext db) : base(db)
    {

    }
}
