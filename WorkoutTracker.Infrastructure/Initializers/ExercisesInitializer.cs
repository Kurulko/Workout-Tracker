using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Repositories.Exercises;
using WorkoutTracker.Application.Interfaces.Repositories.Muscles;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Enums;

namespace WorkoutTracker.Infrastructure.Initializers;

internal class ExercisesInitializer
{
    public static async Task<Exercise> InitializeAsync(IExerciseRepository exerciseRepository, IMuscleRepository muscleRepository, IEquipmentRepository equipmentRepository, string name, string? image, string[] equipmentNames, ExerciseType exerciseType, string[] muscleNames)
    {
        var exercise = await exerciseRepository.GetByNameAsync(name);

        if (exercise is not null)
            return exercise;

        exercise = new ()
        {
            Name = name,
            Image = image,
            Type = exerciseType
        };

        exercise.WorkingMuscles = await GetMuscles(muscleRepository, muscleNames);
        exercise.Equipments = await GetEquipments(equipmentRepository, equipmentNames);

        await exerciseRepository.AddAsync(exercise);
        return exercise;
    }

    static async Task<ICollection<Muscle>> GetMuscles(IMuscleRepository muscleRepository, string[] muscleNames)
    {
        var muscles = new List<Muscle>();

        foreach (string muscleName in muscleNames)
        {
            var muscle = await ArgumentValidator.EnsureExistsByNameAsync(muscleRepository.GetByNameAsync, muscleName, nameof(Muscle));
            muscles.Add(muscle);
        }

        return muscles;
    }

    static async Task<ICollection<Equipment>> GetEquipments(IEquipmentRepository equipmentRepository, string[] equipmentNames)
    {
        var equipments = new List<Equipment>();

        foreach (string equipmentName in equipmentNames)
        {
            var equipment = await ArgumentValidator.EnsureExistsByNameAsync(equipmentRepository.GetByNameAsync, equipmentName, nameof(Equipment));
            equipments.Add(equipment);
        }

        return equipments;
    }
}
