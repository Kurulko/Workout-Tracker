using System.Linq.Dynamic.Core;

namespace WorkoutTrackerAPI.Extentions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> models, string attribute, OrderBy orderBy)
        => models.AsQueryable().OrderBy($"{attribute} {orderBy}");

    public static int CountOrDefault<T>(this IEnumerable<T>? models)
        => models?.Count() ?? default;

    public static IEnumerable<T> GetModelsOrEmpty<T>(this IEnumerable<T>? models)
        => models ?? Enumerable.Empty<T>();
}