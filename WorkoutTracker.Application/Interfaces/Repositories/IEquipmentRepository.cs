using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Application.Interfaces.Repositories;

public interface IEquipmentRepository : IBaseWorkoutRepository<Equipment>
{
    Task<Equipment?> GetEquipmentByIdWithDetailsAsync(long key);
    Task<Equipment?> GetEquipmentByNameWithDetailsAsync(string name);
}