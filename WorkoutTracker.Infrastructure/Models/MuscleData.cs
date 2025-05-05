using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkoutTracker.Infrastructure.Models;

internal class MuscleData
{
    public string Name { get; set; } = null!;
    public string? Image { get; set; }
    public bool IsMeasurable { get; set; }
    public List<MuscleData>? Children { get; set; }
}