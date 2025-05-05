using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Persistence.Context;

namespace WorkoutTracker.Persistence.Repositories.Users;

internal class UserDetailsRepository : DbModelRepository<UserDetails>, IUserDetailsRepository
{
    public UserDetailsRepository(WorkoutDbContext db) : base(db)
    {

    }
}