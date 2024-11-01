﻿using System.ComponentModel.DataAnnotations;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public class EquipmentDTO
{
    public long Id { get; set; }

    [Required(ErrorMessage = "{0} is required")]
    [MinLength(3, ErrorMessage = "{0} must be longer than {1} characters")]
    [MaxLength(50, ErrorMessage = "{0} must be shorter than {1} characters")]
    public string Name { get; set; } = null!;

    public byte[]? Image { get; set; }
    public bool IsOwnedByUser { get; set; }
    public IEnumerable<Exercise>? Exercises { get; set; } = null!;
}
