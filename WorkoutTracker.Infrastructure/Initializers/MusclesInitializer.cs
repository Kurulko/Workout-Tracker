using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Infrastructure.Models;

namespace WorkoutTracker.Infrastructure.Initializers;

internal class MusclesInitializer
{
    public static async Task<Muscle> InitializeAsync(IMuscleRepository muscleRepository, MuscleData muscleData, Muscle? parentMuscle, CancellationToken cancellationToken = default)
    {
        var muscle = await muscleRepository.GetByNameAsync(muscleData.Name, cancellationToken);

        if (muscle is not null)
            return muscle;

        muscle = new Muscle()
        {
            Name = muscleData.Name,
            IsMeasurable = muscleData.IsMeasurable,
            Image = muscleData.Image,
            ParentMuscle = parentMuscle
        };

        await muscleRepository.AddAsync(muscle, cancellationToken);

        if (muscleData.Children is not null)
        {
            foreach (MuscleData child in muscleData.Children)
            {
                await InitializeAsync(muscleRepository, child, muscle, cancellationToken);
            }
        }

        return muscle;
    }
}