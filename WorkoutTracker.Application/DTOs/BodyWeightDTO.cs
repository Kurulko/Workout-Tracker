using System.ComponentModel.DataAnnotations;
using WorkoutTracker.Application.Common.ValidationAttributes;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.DTOs;

public class BodyWeightDTO
{
    public long Id { get; set; }

    [DateNotInFuture]
    public DateTime Date { get; set; }
    public ModelWeight Weight { get; set; }
}
