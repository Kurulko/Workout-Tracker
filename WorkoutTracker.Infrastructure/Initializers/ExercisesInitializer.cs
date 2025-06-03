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
    public static async Task<Exercise> InitializeAsync(
        IExerciseRepository exerciseRepository, 
        IMuscleRepository muscleRepository, 
        IEquipmentRepository equipmentRepository, 
        IExerciseAliasRepository exerciseAliasRepository, 
        string name, 
        string? image, 
        string[] equipmentNames, 
        ExerciseType exerciseType, 
        string[] muscleNames, 
        string[]? aliasesStr = null, 
        CancellationToken cancellationToken = default)
    {
        var exercise = await exerciseRepository.GetByNameAsync(name, cancellationToken);

        if (exercise != null)
            return exercise;

        exercise = new ()
        {
            Name = name,
            Image = image,
            Type = exerciseType
        };

        exercise.WorkingMuscles = await GetMusclesAsync(muscleRepository, muscleNames, cancellationToken);
        exercise.Equipments = await GetEquipmentsAsync(equipmentRepository, equipmentNames, cancellationToken);

        await exerciseRepository.AddAsync(exercise, cancellationToken);

        if (aliasesStr != null && aliasesStr.Any())
            await AddExerciseAliasesAsync(exerciseAliasRepository, exercise.Id, aliasesStr, cancellationToken);

        return exercise;
    }

    static async Task<ICollection<Muscle>> GetMusclesAsync(IMuscleRepository muscleRepository, string[] muscleNames, CancellationToken cancellationToken)
    {
        var muscles = new List<Muscle>();

        foreach (string muscleName in muscleNames)
        {
            var muscle = await ArgumentValidator.EnsureExistsByNameAsync(muscleRepository.GetByNameAsync, muscleName, nameof(Muscle), cancellationToken);
            muscles.Add(muscle);
        }

        return muscles;
    }

    static async Task<ICollection<Equipment>> GetEquipmentsAsync(IEquipmentRepository equipmentRepository, string[] equipmentNames, CancellationToken cancellationToken)
    {
        var equipments = new List<Equipment>();

        foreach (string equipmentName in equipmentNames)
        {
            var equipment = await ArgumentValidator.EnsureExistsByNameAsync(equipmentRepository.GetByNameAsync, equipmentName, nameof(Equipment), cancellationToken);
            equipments.Add(equipment);
        }

        return equipments;
    }

    static async Task AddExerciseAliasesAsync(IExerciseAliasRepository exerciseAliasRepository, long exerciseId, string[] aliasesStr, CancellationToken cancellationToken)
    {
        var aliases = aliasesStr.Select(alias => new ExerciseAlias() { Name = alias, ExerciseId = exerciseId });

        await exerciseAliasRepository.AddRangeAsync(aliases, cancellationToken);
    }
}
