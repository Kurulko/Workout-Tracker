using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WorkoutTracker.Domain.Enums;
using WorkoutTracker.Domain.ValueObjects;

namespace WorkoutTracker.Persistence.Extensions;

internal static class ModelBuilderExtensions
{
    public static void ApplyModelWeightConversion<TEntity>(this ModelBuilder modelBuilder, Expression<Func<TEntity, ModelWeight?>> propertyExpression) where TEntity : class
    {
        modelBuilder.Entity<TEntity>()
            .Property(propertyExpression)
            .HasConversion(
                v => SerializeModelWeight(v), // Serialize ModelWeight to string
                v => DeserializeModelWeight(v)); // Deserialize ModelWeight
    }

    private static string? SerializeModelWeight(ModelWeight? weight)
    {
        if (weight is not ModelWeight _weight)
            return null;

        return string.Format("{0} {1}", _weight.Weight, _weight.WeightType.ToString());
    }

    private static ModelWeight? DeserializeModelWeight(string? value)
    {
        if (value is null)
            return null;

        var index = value.IndexOf(' ');
        return new ModelWeight
        {
            Weight = double.Parse(value.Substring(0, index)),
            WeightType = Enum.Parse<WeightType>(value.Substring(index + 1))
        };
    }

    public static void ApplyModelSizeConversion<TEntity>(this ModelBuilder modelBuilder, Expression<Func<TEntity, ModelSize?>> propertyExpression) where TEntity : class
    {
        modelBuilder.Entity<TEntity>()
            .Property(propertyExpression)
            .HasConversion(
                v => SerializeModelSize(v), // Serialize ModelSize to string
                v => DeserializeModelSize(v)); // Deserialize ModelSize
    }

    private static string? SerializeModelSize(ModelSize? size)
    {
        if (size is not ModelSize _size)
            return null;

        return string.Format("{0} {1}", _size.Size, _size.SizeType.ToString());
    }

    private static ModelSize? DeserializeModelSize(string? value)
    {
        if (value is null)
            return null;

        var index = value.IndexOf(' ');
        return new ModelSize
        {
            Size = double.Parse(value.Substring(0, index)),
            SizeType = Enum.Parse<SizeType>(value.Substring(index + 1))
        };
    }
}