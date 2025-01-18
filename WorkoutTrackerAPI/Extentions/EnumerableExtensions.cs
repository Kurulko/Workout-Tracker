using System.Linq.Dynamic.Core;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Extentions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> models, string attribute, OrderBy orderBy)
        => models.AsQueryable().OrderBy($"{attribute} {orderBy}");

    public static int CountOrDefault<T>(this IEnumerable<T>? models)
        => models?.Count() ?? default;

    public static IEnumerable<T> GetModelsOrEmpty<T>(this IEnumerable<T>? models)
        => models ?? Enumerable.Empty<T>();

    public static bool Contains<T>(this IEnumerable<T> models, IEnumerable<T> containedModels)
        => containedModels.All(containedModel => models.Contains(containedModel));

    public static IEnumerable<Exercise> GetExercises(this IEnumerable<ExerciseRecordGroup> exerciseRecordGroups)
        => exerciseRecordGroups.SelectMany(es => es.ExerciseRecords).Select(er => er.Exercise).Distinct()!;
    public static IEnumerable<Exercise> GetExercises(this IEnumerable<ExerciseSetGroup> exerciseSetGroups)
        => exerciseSetGroups.SelectMany(es => es.ExerciseSets).Select(er => er.Exercise).Distinct()!;

    public static IEnumerable<Equipment> GetEquipments(this IEnumerable<ExerciseRecordGroup> exerciseRecordGroups)
        => exerciseRecordGroups.SelectMany(es => es.ExerciseRecords).Select(er => er.Exercise).SelectMany(e => e!.Equipments).Distinct()!;
    public static IEnumerable<Equipment> GetEquipments(this IEnumerable<ExerciseSetGroup> exerciseSetGroups)
        => exerciseSetGroups.SelectMany(es => es.ExerciseSets).Select(er => er.Exercise).SelectMany(e => e!.Equipments).Distinct()!;

    public static IEnumerable<Muscle> GetMuscles(this IEnumerable<ExerciseRecordGroup> exerciseRecordGroups)
        => exerciseRecordGroups.SelectMany(es => es.ExerciseRecords).Select(er => er.Exercise).SelectMany(e => e!.WorkingMuscles).Distinct()!;
    public static IEnumerable<Muscle> GetMuscles(this IEnumerable<ExerciseSetGroup> exerciseSetGroups)
        => exerciseSetGroups.SelectMany(es => es.ExerciseSets).Select(er => er.Exercise).SelectMany(e => e!.WorkingMuscles).Distinct()!;


    public static ModelWeight GetTotalWeightValue(this IEnumerable<Workout> workouts)
    {
        ModelWeight sum = new();

        foreach (Workout workout in workouts)
        {
            sum += workout.WorkoutRecords!.GetTotalWeightValue();
        }

        return sum;
    }

    public static ModelWeight GetTotalWeightValue(this IEnumerable<WorkoutRecord> workoutRecords)
    {
        ModelWeight sum = new();

        foreach (WorkoutRecord workoutRecord in workoutRecords)
        {
            sum += workoutRecord.ExerciseRecordGroups.GetTotalWeightValue();
        }

        return sum;
    }

    public static ModelWeight GetTotalWeightValue(this IEnumerable<ExerciseRecordGroup> exerciseRecordGroups)
    {
        ModelWeight sum = new();

        foreach (ExerciseRecordGroup exerciseRecordGroup in exerciseRecordGroups)
        {
            sum += exerciseRecordGroup.ExerciseRecords.GetTotalWeightValue();
        }

        return sum;
    }

    public static ModelWeight GetTotalWeightValue(this IEnumerable<ExerciseSetGroup> exerciseSetGroups)
    {
        ModelWeight sum = new();

        foreach (ExerciseSetGroup exerciseSetGroup in exerciseSetGroups)
        {
            sum += exerciseSetGroup.ExerciseSets.GetTotalWeightValue();
        }

        return sum;
    }

    public static ModelWeight GetTotalWeightValue(this IEnumerable<ExerciseSet> exerciseSets)
    {
        ModelWeight sum = new();

        foreach (var exerciseSet in exerciseSets)
        {
            sum += exerciseSet.GetTotalWeightValue();
        }

        return sum;
    }

    public static ModelWeight GetTotalWeightValue(this IEnumerable<ExerciseRecord> exerciseRecords)
    {
        ModelWeight sum = new();

        foreach (var exerciseRecord in exerciseRecords)
        {
            sum += exerciseRecord.GetTotalWeightValue();
        }

        return sum;
    }

    public static ModelWeight GetTotalWeightValue(this ExerciseRecord exerciseRecord)
    {
        if (exerciseRecord.Weight is ModelWeight weight && exerciseRecord.Reps is int reps)
            return weight * reps;

        return new ModelWeight();
    }

    public static ModelWeight GetTotalWeightValue(this ExerciseSet exerciseSet)
    {
        if (exerciseSet.Weight is ModelWeight weight && exerciseSet.Reps is int reps)
            return weight * reps;

        return new ModelWeight();
    }


    public static TimeSpan GetTotalTimeValue(this IEnumerable<Workout> workouts)
    {
        TimeSpan sum = new();

        foreach (Workout workout in workouts)
        {
            sum += workout.WorkoutRecords!.GetTotalTimeValue();
        }

        return sum;
    }

    public static TimeSpan GetTotalTimeValue(this IEnumerable<WorkoutRecord> workoutRecords)
    {
        TimeSpan sum = new();

        foreach (WorkoutRecord workoutRecord in workoutRecords)
        {
            sum += workoutRecord.ExerciseRecordGroups.GetTotalTimeValue();
        }

        return sum;
    }

    public static TimeSpan GetTotalTimeValue(this IEnumerable<ExerciseRecordGroup> exerciseRecordGroups)
    {
        TimeSpan sum = new();

        foreach (ExerciseRecordGroup exerciseRecordGroup in exerciseRecordGroups)
        {
            sum += exerciseRecordGroup.ExerciseRecords.GetTotalTimeValue();
        }

        return sum;
    }

    public static TimeSpan GetTotalTimeValue(this IEnumerable<ExerciseRecord> exerciseRecords)
    {
        TimeSpan sum = new();

        foreach (ExerciseRecord exerciseRecord in exerciseRecords)
        {
            sum += exerciseRecord.GetTotalTimeValue();
        }

        return sum;
    }

    public static TimeSpan GetTotalTimeValue(this ExerciseRecord exerciseRecord)
    {
        if (exerciseRecord.Time is TimeSpan time)
        {
            if (exerciseRecord.Reps is int reps)
                return time * reps;
            else
                return time;
        }

        return new TimeSpan();
    }

    public static TimeSpan GetTotalTimeValue(this ExerciseSet exerciseSet)
    {
        if (exerciseSet.Time is TimeSpan time)
        {
            if (exerciseSet.Reps is int reps)
                return time * reps;
            else
                return time;
        }

        return new TimeSpan();
    }


    public static int GetTotalRepsValue(this IEnumerable<Workout> workouts)
        => workouts.Sum(wr => wr.WorkoutRecords!.GetTotalRepsValue());

    public static int GetTotalRepsValue(this IEnumerable<WorkoutRecord> workoutRecords)
        => workoutRecords.Sum(wr => wr.ExerciseRecordGroups.GetTotalRepsValue());

    public static int GetTotalRepsValue(this IEnumerable<ExerciseRecordGroup> exerciseRecordGroups)
        => exerciseRecordGroups.Sum(erg => erg.ExerciseRecords.GetTotalRepsValue());

    public static int GetTotalRepsValue(this IEnumerable<ExerciseRecord> exerciseRecords)
        => exerciseRecords.Where(er => er.Reps is not null).Sum(er => er.Reps!.Value);


    public static ModelWeight? GetTotalWeightValue(this Exercise exercise)
    {
        if (exercise.Type == ExerciseType.WeightAndReps)
            return exercise.ExerciseRecords!.GetTotalWeightValue();

        return null;
    }

    public static TimeSpan? GetTotalTimeValue(this Exercise exercise)
    {
        if (exercise.Type == ExerciseType.Time)
            return exercise.ExerciseRecords!.GetTotalTimeValue();

        return null;
    }

    public static int? GetTotalRepsValue(this Exercise exercise)
    {
        if (exercise.Type == ExerciseType.Reps)
            return exercise.ExerciseRecords!.GetTotalRepsValue();

        return null;
    }


    public static TimeSpan GetTotalTime(this IEnumerable<Workout> workouts)
    {
        TimeSpan sum = new();

        foreach (Workout workout in workouts)
        {
            sum += workout.WorkoutRecords!.GetTotalTime();
        }

        return sum;
    }

    public static TimeSpan GetTotalTime(this IEnumerable<WorkoutRecord> workoutRecords)
    {
        TimeSpan sum = new();

        foreach (WorkoutRecord workoutRecord in workoutRecords)
        {
            sum += workoutRecord.Time;
        }

        return sum;
    }
}