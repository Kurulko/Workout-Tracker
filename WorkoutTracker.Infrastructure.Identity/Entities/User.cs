using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Infrastructure.Identity.Entities;

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
