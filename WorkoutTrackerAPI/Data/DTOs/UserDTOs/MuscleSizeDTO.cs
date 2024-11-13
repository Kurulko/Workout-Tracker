﻿using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs;

public class MuscleSizeDTO
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public float Size { get; set; }
    public SizeType SizeType { get; set; }

    public long MuscleId { get; set; }
    public string? MuscleName { get; set; }
}
