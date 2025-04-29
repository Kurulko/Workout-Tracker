using WorkoutTrackerAPI.Data;

namespace WorkoutTrackerAPI.Extentions;

public static class MathExtentions
{
    public static ModelWeight Round(this ModelWeight modelWeight, int digits)
    {
        modelWeight.Weight = Math.Round(modelWeight.Weight, digits);
        return modelWeight;
    }
}
