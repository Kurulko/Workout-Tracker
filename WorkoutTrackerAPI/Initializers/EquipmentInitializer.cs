using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;

namespace WorkoutTrackerAPI.Initializers;

public class EquipmentInitializer
{
    public static async Task<Equipment> InitializeAsync(EquipmentRepository equipmentRepository, string name)
    {
        var equipment = await equipmentRepository.GetByNameAsync(name);

        if (equipment is not null)
            return equipment;

        equipment = new Equipment () { Name = name };
        await equipmentRepository.AddAsync(equipment);
        return equipment;
    }
}
