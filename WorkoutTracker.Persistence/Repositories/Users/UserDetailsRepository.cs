using WorkoutTracker.Persistence.Repositories.Base;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Persistence.Context;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Persistence.Repositories.Users;

internal class UserDetailsRepository : DbModelRepository<UserDetails>, IUserDetailsRepository
{
    public UserDetailsRepository(WorkoutDbContext db) : base(db)
    {

    }
}