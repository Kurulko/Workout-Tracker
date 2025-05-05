using WorkoutTracker.Application.Interfaces.Services.Progresses;
using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Application.Common.Extensions;

namespace WorkoutTracker.Infrastructure.Services.Progresses;

internal class BodyWeightProgressService : IBodyWeightProgressService
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
