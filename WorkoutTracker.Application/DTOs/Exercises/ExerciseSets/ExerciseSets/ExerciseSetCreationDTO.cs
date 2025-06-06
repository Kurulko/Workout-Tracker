﻿using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSets;

public class ExerciseSetCreationDTO
{
    public ModelWeight? Weight { get; set; }
    public TimeSpanModel? Time { get; set; }
    public int? Reps { get; set; }

    public long ExerciseId { get; set; }
}
