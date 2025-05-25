using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Interfaces.Repositories;

public interface IUserDetailsRepository : IBaseRepository<UserDetails>
{
}