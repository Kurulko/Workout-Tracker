using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Persistence.Configurations.Muscles;

internal class MuscleConfiguration : BaseWorkoutModelConfiguration<Muscle>
{
    public override void Configure(EntityTypeBuilder<Muscle> builder)
    {
        base.Configure(builder);

        builder
            .HasIndex(m => m.Name)
            .IsUnique();

        builder.Property(t => t.Image).IsRequired(false);
        builder.Property(t => t.IsMeasurable).HasDefaultValue(false);

        builder.HasOne(m => m.ParentMuscle)
            .WithMany(m => m.ChildMuscles)
            .HasForeignKey(m => m.ParentMuscleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.MuscleSizes)
            .WithOne(ea => ea.Muscle)
            .HasForeignKey(a => a.MuscleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
