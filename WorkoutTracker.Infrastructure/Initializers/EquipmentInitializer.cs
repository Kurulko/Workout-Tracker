﻿using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Infrastructure.Initializers;

internal class EquipmentInitializer
{
    public static async Task<Equipment> InitializeAsync(IEquipmentRepository equipmentRepository, string name, CancellationToken cancellationToken = default)
    {
        var equipment = await equipmentRepository.GetByNameAsync(name, cancellationToken);

        if (equipment is not null)
            return equipment;

        equipment = new Equipment () { Name = name };
        await equipmentRepository.AddAsync(equipment, cancellationToken);
        return equipment;
    }
}
