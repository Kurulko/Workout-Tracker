using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Data.Models;

public class User : IdentityUser
{
    public int CountOfTrainings { get; set; }
    public DateTime Registered { get; set; }
    public DateTime? StartedWorkingOut { get; set; }

    public UserDetails? UserDetails { get; set; }
    public IEnumerable<BodyWeight>? BodyWeights { get; set; }
    public IEnumerable<MuscleSize>? MuscleSizes { get; set; }
    public IEnumerable<Workout>? Workouts { get; set; }
    public IEnumerable<ExerciseRecord>? ExerciseRecords { get; set; }
    public IEnumerable<WorkoutRecord>? WorkoutRecords { get; set; }
    public IEnumerable<Exercise>? CreatedExercises { get; set; }
    public IEnumerable<Equipment>? UserEquipments { get; set; }
}
