using System.ComponentModel.DataAnnotations;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.ValidationAttributes;

namespace WorkoutTrackerAPI.Data.DTOs;

public class BodyWeightDTO
{
    public long Id { get; set; }

    [DateNotInFuture]
    public DateTime Date { get; set; }

    [PositiveNumber(ErrorMessage = "Weight must be a positive number.")]
    public float Weight { get; set; }

    public WeightType WeightType { get; set; }
}
