using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;

public class ExerciseRecordGroup : IDbModel
{
    public long Id { get; set; }

    public long ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    public long WorkoutRecordId { get; set; }
    public WorkoutRecord? WorkoutRecord { get; set; }

    public IEnumerable<ExerciseRecord> ExerciseRecords { get; set; } = null!;
}