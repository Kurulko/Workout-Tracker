using System;
using WorkoutTrackerAPI.Data.Enums;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data;

public struct ModelWeight : IComparable<ModelWeight>
{
    public double Weight { get; set; }
    public WeightType WeightType { get; set; }


    const double oneKilogramInPounds = 0.453592;
    public double GetWeightInKilos()
    {
        if (WeightType == WeightType.Kilogram)
            return Weight;

        return Weight * oneKilogramInPounds;
    }

    public static ModelWeight GetModelWeightInKilos(ModelWeight modelWeight)
    {
        if (modelWeight.WeightType == WeightType.Kilogram)
            return modelWeight;

        double weightInKilos = modelWeight.GetWeightInKilos();
        return new ModelWeight() { WeightType = WeightType.Kilogram, Weight = weightInKilos };
    }

    const double onePoundInKilograms = 2.20462;
    public double GetWeightInPounds()
    {
        if (WeightType == WeightType.Pound)
            return Weight;

        return Weight * onePoundInKilograms;
    }

    public static ModelWeight GetModelWeightInPounds(ModelWeight modelWeight)
    {
        if (modelWeight.WeightType == WeightType.Pound)
            return modelWeight;

        double weightInPounds = modelWeight.GetWeightInPounds();
        return new ModelWeight() { WeightType = WeightType.Pound, Weight = weightInPounds };
    }

    public static ModelWeight operator +(ModelWeight weight1, ModelWeight weight2)
    {
        if (weight1.WeightType == WeightType.Kilogram)
        {
            double weight2InKilos = weight2.GetWeightInKilos();
            return new ModelWeight() { WeightType = WeightType.Kilogram, Weight = weight1.Weight + weight2InKilos };
        }
        else
        {
            double weight2InPounds = weight2.GetWeightInPounds();
            return new ModelWeight() { WeightType = WeightType.Pound, Weight = weight1.Weight + weight2InPounds };
        }
    }

    public static ModelWeight operator -(ModelWeight weight1, ModelWeight weight2)
    {
        if (weight1.WeightType == WeightType.Kilogram)
        {
            double weight2InKilos = weight2.GetWeightInKilos();
            return new ModelWeight() { WeightType = WeightType.Kilogram, Weight = weight1.Weight - weight2InKilos };
        }
        else
        {
            double weight2InPounds = weight2.GetWeightInPounds();
            return new ModelWeight() { WeightType = WeightType.Pound, Weight = weight1.Weight - weight2InPounds };
        }
    }

    public static ModelWeight operator *(ModelWeight weight, int value)
    {
        weight.Weight *= value;
        return weight;
    }

    public static ModelWeight operator /(ModelWeight weight, int value)
    {
        weight.Weight /= value;
        return weight;
    }


    public int CompareTo(ModelWeight other)
    {
        double weightInKilos = GetWeightInKilos(), otherWeightInKilos = other.GetWeightInKilos();
        return weightInKilos.CompareTo(otherWeightInKilos);
    }
}