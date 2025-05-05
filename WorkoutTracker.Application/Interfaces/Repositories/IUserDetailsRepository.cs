using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories;

public interface IUserDetailsRepository : IBaseRepository<UserDetails>
{
}