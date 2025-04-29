using Microsoft.EntityFrameworkCore;
using WorkoutTrackerAPI.Data.Enums;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Data.Models;

public class Exercise : WorkoutModel
{
    public string? Image { get; set; }
    public string? Description { get; set; }
    public ExerciseType Type { get; set; }

    public string? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public ICollection<Equipment> Equipments { get; set; } = null!;
    public ICollection<Muscle> WorkingMuscles { get; set; } = null!;
    public IEnumerable<ExerciseAlias>? ExerciseAliases { get; set; }
    public IEnumerable<ExerciseRecord>? ExerciseRecords { get; set; }
    public IEnumerable<ExerciseRecordGroup>? ExerciseRecordGroups { get; set; }
    public IEnumerable<ExerciseSetGroup>? ExerciseSetGroups { get; set; }
}
