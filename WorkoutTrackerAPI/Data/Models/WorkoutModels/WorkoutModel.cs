using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace WorkoutTrackerAPI.Data.Models;

[Index(nameof(Name), IsUnique = true)]
public abstract class WorkoutModel : IDbModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be longer than {1} characters")]
    [MaxLength(50, ErrorMessage = "{0} must be shorter than {1} characters")]
    public string Name { get; set; } = null!;
}