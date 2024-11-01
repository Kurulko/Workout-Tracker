using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WorkoutTrackerAPI.Data.Models;

[Index(nameof(Name), IsUnique = true)]
public abstract class WorkoutModel : IDbModel
{
    public long Id { get; set; }
    public string Name { get; set; } = null!;
}