using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data.Models;

public class User : IdentityUser, IBaseModel
{
    public int CountOfTrainings { get; set; }
    public DateTime Registered { get; set; }
    public DateTime? StartedWorkingOut { get; set; }
    public string? UsedUserId { get; set; }

    public IEnumerable<BodyWeight>? BodyWeights { get; set; }
    public IEnumerable<MuscleSize>? MuscleSizes { get; set; }
    public IEnumerable<Workout>? Workouts { get; set; }
    public IEnumerable<ExerciseRecord>? ExerciseRecords { get; set; }
    public IEnumerable<Exercise>? CreatedExercises { get; set; }

    //public override bool Equals(object? obj)
    //    => obj is User user && UserName == user.UserName;

    //public override int GetHashCode()
    //    => UserName.GetHashCode();
}
