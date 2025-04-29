using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Services.ProgressServices.BodyWeightProgressServices;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Services.ProgressServices;

public class BodyWeightProgressService : IBodyWeightProgressService
{
    public BodyWeightProgress CalculateBodyWeightProgress(IEnumerable<BodyWeight> bodyWeights)
    {
        if (!bodyWeights.Any())
            return new BodyWeightProgress();

        BodyWeightProgress bodyWeightProgress = new();

        var weights = bodyWeights.Select(bw => bw.Weight);

        bodyWeightProgress.AverageBodyWeight = weights.Average().Round(1);
        bodyWeightProgress.MinBodyWeight = weights.Min();
        bodyWeightProgress.MaxBodyWeight = weights.Max();

        return bodyWeightProgress;
    }
}
