using WorkoutTracker.Application.Models.Progresses;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Interfaces.Services.Progresses;

public interface IBodyWeightProgressService : IBaseService
{
    BodyWeightProgress CalculateBodyWeightProgress(IEnumerable<BodyWeight> bodyWeights);
}
