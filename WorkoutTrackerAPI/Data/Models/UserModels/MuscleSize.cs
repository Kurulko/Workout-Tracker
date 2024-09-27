using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WorkoutTrackerAPI.Data.Models.UserModels;

public class MuscleSize : IDbModel
{
    public long Id { get; set; }
    public DateOnly Date { get; set; }
    public float Size { get; set; }
    public SizeType SizeType { get; set; }

    public long MuscleId { get; set; }
    public Muscle? Muscle { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }


    const double oneCentimeterInInches = 0.393701;
    public static double GetMuscleSizeInInches(MuscleSize muscleSize)
    {
        if (muscleSize.SizeType == SizeType.Inch)
            return muscleSize.Size;

        return muscleSize.Size * oneCentimeterInInches;
    }

    const double oneInchInCentimeters = 2.54;
    public static double GetMuscleSizeInCentimeters(MuscleSize muscleSize)
    {
        if (muscleSize.SizeType == SizeType.Centimeter)
            return muscleSize.Size;

        return muscleSize.Size * oneInchInCentimeters;
    }
}

public enum SizeType
{
    Centimeter, Inch
}
// centimeter ~ cm
