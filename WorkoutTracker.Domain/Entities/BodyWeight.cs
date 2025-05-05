using WorkoutTracker.Domain.Base;
using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Domain.Entities;

public class BodyWeight : IDbModel
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public ModelWeight Weight { get; set; }

    public string UserId { get; set; } = null!;
}

