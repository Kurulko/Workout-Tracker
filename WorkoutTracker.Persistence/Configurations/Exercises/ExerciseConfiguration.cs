using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Persistence.Configurations.Base;

namespace WorkoutTracker.Persistence.Configurations.Exercises;

internal class ExerciseConfiguration : BaseWorkoutModelConfiguration<Exercise>
{
    public override void Configure(EntityTypeBuilder<Exercise> builder)
    {
        base.Configure(builder);

        builder
            .HasIndex(e => new { e.Name, e.CreatedByUserId })
            .IsUnique();

        builder.Property(t => t.Image).IsRequired(false);
        builder.Property(t => t.Type).IsRequired();
        builder.Property(t => t.Description)
               .HasMaxLength(500)
               .IsRequired(false);


        builder.HasMany(e => e.Equipments)
           .WithMany(eq => eq.Exercises)
           .UsingEntity(
                x => x.HasOne(typeof(Equipment)).WithMany().OnDelete(DeleteBehavior.Cascade),
                x => x.HasOne(typeof(Exercise)).WithMany().OnDelete(DeleteBehavior.Cascade)
            );

        builder.HasMany(e => e.WorkingMuscles)
           .WithMany(eq => eq.Exercises)
           .UsingEntity(
                x => x.HasOne(typeof(Muscle)).WithMany().OnDelete(DeleteBehavior.Cascade),
                x => x.HasOne(typeof(Exercise)).WithMany().OnDelete(DeleteBehavior.Cascade)
            );

        builder.HasMany(e => e.ExerciseAliases)
            .WithOne(ea => ea.Exercise)
            .HasForeignKey(a => a.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ExerciseRecords)
            .WithOne(er => er.Exercise)
            .HasForeignKey(r => r.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ExerciseRecordGroups)
            .WithOne(erg => erg.Exercise)
            .HasForeignKey(grp => grp.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.ExerciseSetGroups)
            .WithOne(esg => esg.Exercise)
            .HasForeignKey(sg => sg.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
