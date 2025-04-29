using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models.ProgressModels;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Services.ProgressServices.BodyWeightProgressServices;

public interface IBodyWeightProgressService
{
    BodyWeightProgress CalculateBodyWeightProgress(IEnumerable<BodyWeight> bodyWeights);
}
