using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Domain.Entities.Exercises;

public class Exercise : BaseWorkoutModel
{
    public string? Image { get; set; }
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }

    public string? CreatedByUserId { get; set; }

    public ICollection<Equipment> Equipments { get; set; } = null!;
    public ICollection<Muscle> WorkingMuscles { get; set; } = null!;

    public IEnumerable<ExerciseAlias>? ExerciseAliases { get; set; }
    public IEnumerable<ExerciseRecord>? ExerciseRecords { get; set; }
    public IEnumerable<ExerciseRecordGroup>? ExerciseRecordGroups { get; set; }
    public IEnumerable<ExerciseSetGroup>? ExerciseSetGroups { get; set; }
}
