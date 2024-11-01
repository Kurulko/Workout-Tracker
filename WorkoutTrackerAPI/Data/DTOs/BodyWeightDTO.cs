using System.ComponentModel.DataAnnotations;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs;

public class BodyWeightDTO : IDbModel
{
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public float Weight { get; set; }
    public WeightType WeightType { get; set; }
}

