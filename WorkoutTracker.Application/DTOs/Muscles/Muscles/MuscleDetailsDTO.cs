using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;

namespace WorkoutTracker.Application.DTOs.Muscles.Muscles;

public class MuscleDetailsDTO
{
    public MuscleDTO Muscle { get; set; } = null!;

    public IEnumerable<ExerciseDTO>? Exercises { get; set; }
    public IEnumerable<MuscleSizeDTO>? MuscleSizes { get; set; }
}
