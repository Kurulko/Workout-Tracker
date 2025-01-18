using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

namespace WorkoutTrackerAPI.Repositories.WorkoutRepositories;

public class ExerciseSetRepository : DbModelRepository<ExerciseSet>
{
    public ExerciseSetRepository(WorkoutDbContext db) : base(db)
    {

    }
}

