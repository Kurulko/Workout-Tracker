using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Persistence.Configurations.Base;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Persistence.Configurations.Workouts;

internal class WorkoutConfiguration : BaseWorkoutModelConfiguration<Workout>
{
    public override void Configure(EntityTypeBuilder<Workout> builder)
    {
        base.Configure(builder);

        builder.ToTable(b => {
            b.HasCheckConstraint("CK_Workout_Created", $"{nameof(Workout.Created)} <= GETDATE()");
        });

        builder
            .HasIndex(w => new { w.Name, w.UserId })
            .IsUnique();

        builder.Property(er => er.Created).IsRequired();
        builder.Property(er => er.IsPinned).HasDefaultValue(false).IsRequired();
        builder.Property(er => er.CountOfTrainings).HasDefaultValue(0).IsRequired();

        builder.Property(t => t.Description)
               .HasMaxLength(500)
               .IsRequired(false);


        builder.HasMany(e => e.ExerciseSetGroups)
           .WithOne(esg => esg.Workout)
           .HasForeignKey(esg => esg.WorkoutId) 
           .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.WorkoutRecords)
           .WithOne(esg => esg.Workout)
           .HasForeignKey(esg => esg.WorkoutId) 
           .OnDelete(DeleteBehavior.Cascade);
    }
}
