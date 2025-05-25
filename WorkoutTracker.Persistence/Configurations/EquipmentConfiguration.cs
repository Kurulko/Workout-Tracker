using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Persistence.Configurations;

internal class EquipmentConfiguration : BaseWorkoutModelConfiguration<Equipment>
{
    public override void Configure(EntityTypeBuilder<Equipment> builder)
    {
        base.Configure(builder);

        builder
            .HasIndex(e => new { e.Name, e.OwnedByUserId })
            .IsUnique();

        builder.Property(t => t.Image).IsRequired(false);
    }
}
