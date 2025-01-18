using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.MuscleServices;
using WorkoutTrackerAPI.Services.RoleServices;

namespace WorkoutTrackerAPI.Initializers;

public class MusclesInitializer
{
    public static async Task<Muscle> InitializeAsync(MuscleRepository muscleRepository, MuscleData muscleData, Muscle? parentMuscle)
    {
        var muscle = await muscleRepository.GetByNameAsync(muscleData.Name);

        if (muscle is not null)
            return muscle;

        muscle = new Muscle()
        {
            Name = muscleData.Name,
            IsMeasurable = muscleData.IsMeasurable,
            Image = muscleData.Image,
            ParentMuscle = parentMuscle
        };

        await muscleRepository.AddAsync(muscle);

        if (muscleData.Children is not null)
        {
            foreach (MuscleData child in muscleData.Children)
            {
                await InitializeAsync(muscleRepository, child, muscle);
            }
        }

        return muscle;
    }
}

public class MuscleData
{
    public string Name { get; set; } = null!;
    public string? Image { get; set; }
    public bool IsMeasurable { get; set; }
    public List<MuscleData>? Children { get; set; }
}
