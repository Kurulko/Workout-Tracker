using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkoutTrackerAPI.Data.Models.UserModels;

public class BodyWeight : IDbModel
{
    [Key]
    public long Id { get; set; }
    public DateTime Date { get; set; }
    public float Weight { get; set; }
    public WeightType WeightType { get; set; }

    public string UserId { get; set; } = null!;
    public User? User { get; set; }


    const double onePoundInKilograms = 2.20462;
    public static double GetBodyWeightInKilos(BodyWeight bodyWeight)
    {
        if (bodyWeight.WeightType == WeightType.Kilogram)
            return bodyWeight.Weight;

        return bodyWeight.Weight * onePoundInKilograms;
    }

    const double oneKilogramInPounds = 0.453592;
    public static double GetBodyWeightInPounds(BodyWeight bodyWeight)
    {
        if (bodyWeight.WeightType == WeightType.Pound)
            return bodyWeight.Weight;

        return bodyWeight.Weight * oneKilogramInPounds;
    }
}

public enum WeightType
{
    Kilogram, Pound
}
// pound  ~ lb, lbm
// kilogram ~ kilo, kg
