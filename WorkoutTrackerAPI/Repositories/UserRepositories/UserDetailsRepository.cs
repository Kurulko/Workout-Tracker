using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Repositories.UserRepositories;

public class UserDetailsRepository : DbModelRepository<UserDetails>
{
    public UserDetailsRepository(WorkoutDbContext db) : base(db)
    {

    }
}