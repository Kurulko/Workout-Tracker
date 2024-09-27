using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Services.MuscleServices;
using WorkoutTrackerAPI.Services.RoleServices;

namespace WorkoutTrackerAPI.Initializers;

public class MusclesInitializer
{
    public static async Task InitializeAsync(MuscleRepository muscleRepository, MuscleData muscleData, Muscle? parentMuscle)
    {
        Muscle muscle = new Muscle
        {
            Name = muscleData.Name,
            ParentMuscle = parentMuscle
        };

        await muscleRepository.AddAsync(muscle);

        if (muscleData.Children is null)
            return;

        foreach (MuscleData child in muscleData.Children)
        {
            await InitializeAsync(muscleRepository, child, muscle);
        }
    }
}

public class MuscleData
{
    public string Name { get; set; } = null!;
    public List<MuscleData>? Children { get; set; }
}
