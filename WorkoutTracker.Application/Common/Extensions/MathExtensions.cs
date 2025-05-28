using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Application.Common.Extensions;

public static class MathExtensions
{
    public static ModelWeight Round(this ModelWeight modelWeight, int digits)
    {
        modelWeight.Weight = Math.Round(modelWeight.Weight, digits);
        return modelWeight;
    }

    public static ModelWeight Sum(this IEnumerable<ModelWeight> modelWeights)
    {
        return modelWeights.Aggregate((w1, w2) => w1 + w2);
    }

    public static ModelWeight Average(this IEnumerable<ModelWeight> modelWeights)
    {
        var sum = modelWeights.Sum();
        var count = modelWeights.Count();
        return sum / count;
    }
}
