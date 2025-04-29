using WorkoutTrackerAPI.Data.Enums;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.WorkoutRepositories;

namespace WorkoutTrackerAPI.Initializers;

public class ExercisesInitializer
{
    public static async Task<Exercise> InitializeAsync(ExerciseRepository exerciseRepository, MuscleRepository muscleRepository, EquipmentRepository equipmentRepository, string name, string? image, string[] equipmentNames, ExerciseType exerciseType, string[] muscleNames)
    {
        var exercise = await exerciseRepository.GetByNameAsync(name);

        if (exercise is not null)
            return exercise;

        exercise = new()
        {
            Name = name,
            Image = image,
            Type = exerciseType
        };

        var muscles = new List<Muscle>();
        foreach (string muscleName in muscleNames)
        {
            var muscle = await muscleRepository.GetByNameAsync(muscleName) ?? throw NotFoundException.NotFoundExceptionByName(nameof(Muscle), muscleName);
            muscles.Add(muscle);
        }
        exercise.WorkingMuscles = muscles;


        var equipments = new List<Equipment>();
        foreach (string equipmentName in equipmentNames)
        {
            var equipment = await equipmentRepository.GetByNameAsync(equipmentName) ?? throw NotFoundException.NotFoundExceptionByName(nameof(Equipment), equipmentName);
            equipments.Add(equipment);
        }
        exercise.Equipments = equipments;


        await exerciseRepository.AddAsync(exercise);
        return exercise;
    }
}
