using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Domain.Entities.Muscles;

public class MuscleSize : IDbModel
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public ModelSize Size { get; set; }

    public long MuscleId { get; set; }
    public Muscle? Muscle { get; set; }

    public string UserId { get; set; } = null!;
}

