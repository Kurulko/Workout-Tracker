using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Domain.Entities.Muscles;
using WorkoutTracker.Domain.Entities.Users;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Extensions;
using WorkoutTracker.Persistence.Extensions;

namespace WorkoutTracker.Persistence.Context;

public class WorkoutDbContext : IdentityDbContext<User>
{
    public DbSet<UserDetails> UsersDetails => Set<UserDetails>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Muscle> Muscles => Set<Muscle>();
    public DbSet<MuscleSize> MuscleSizes => Set<MuscleSize>();
    public DbSet<BodyWeight> BodyWeights => Set<BodyWeight>();
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<ExerciseRecord> ExerciseRecords => Set<ExerciseRecord>();
    public DbSet<ExerciseRecordGroup> ExerciseRecordGroups => Set<ExerciseRecordGroup>();
    public DbSet<ExerciseSet> ExerciseSets => Set<ExerciseSet>();
    public DbSet<ExerciseSetGroup> ExerciseSetGroups => Set<ExerciseSetGroup>();
    public DbSet<WorkoutRecord> WorkoutRecords => Set<WorkoutRecord>();
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<ExerciseAlias> ExerciseAliases => Set<ExerciseAlias>();

    public WorkoutDbContext(DbContextOptions options) : base(options) { }
         //=> Database.EnsureCreated();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        //modelBuilder.ApplyConfiguration(new ModelConfiguration());

        modelBuilder.ApplyModelWeightConversion<BodyWeight>(bw => bw.Weight);
        modelBuilder.ApplyModelSizeConversion<MuscleSize>(ms => ms.Size);
        modelBuilder.ApplyModelWeightConversion<UserDetails>(ud => ud.Weight);
        modelBuilder.ApplyModelSizeConversion<UserDetails>(ud => ud.Height);
        modelBuilder.ApplyModelWeightConversion<ExerciseSet>(m => m.Weight);
        modelBuilder.ApplyModelWeightConversion<ExerciseRecord>(m => m.Weight);

        modelBuilder.Entity<ExerciseSetGroup>()
            .HasMany(w => w.ExerciseSets)
            .WithOne(u => u.ExerciseSetGroup)
            .HasForeignKey(e => e.ExerciseSetGroupId)
            .OnDelete(DeleteBehavior.ClientCascade);

        modelBuilder.Entity<ExerciseRecordGroup>()
            .HasMany(w => w.ExerciseRecords)
            .WithOne(u => u.ExerciseRecordGroup)
            .HasForeignKey(e => e.ExerciseRecordGroupId)
            .OnDelete(DeleteBehavior.ClientCascade);

        modelBuilder.Entity<Workout>()
            .HasMany(w => w.WorkoutRecords)
            .WithOne(u => u.Workout)
            .HasForeignKey(e => e.WorkoutId)
            .OnDelete(DeleteBehavior.ClientCascade);

        //modelBuilder.Entity<Exercise>()
        //    .HasOne(e => e.CreatedByUser)
        //    .WithMany(u => u.CreatedExercises)
        //    .HasForeignKey(e => e.CreatedByUserId)
        //    .OnDelete(DeleteBehavior.SetNull);

        //modelBuilder.Entity<Equipment>()
        //    .HasOne(e => e.OwnedByUser)
        //    .WithMany(u => u.UserEquipments)
        //    .HasForeignKey(e => e.OwnedByUserId)
        //    .OnDelete(DeleteBehavior.SetNull);

        //modelBuilder.Entity<ExerciseRecord>()
        //    .HasOne(er => er.User)
        //    .WithMany(u => u.ExerciseRecords)
        //    .HasForeignKey(er => er.UserId);

        modelBuilder.Entity<ExerciseRecord>()
            .HasOne(er => er.Exercise)
            .WithMany(e => e.ExerciseRecords)
            .HasForeignKey(er => er.ExerciseId);


        modelBuilder.Entity<Workout>()
            .HasIndex(w => new { w.Name, w.UserId })
            .IsUnique();

        modelBuilder.Entity<Equipment>()
            .HasIndex(e => new { e.Name, e.OwnedByUserId })
            .IsUnique();

        modelBuilder.Entity<Exercise>()
            .HasIndex(e => new { e.Name, e.CreatedByUserId })
            .IsUnique();

        modelBuilder.Entity<Muscle>()
            .HasIndex(m => m.Name)
            .IsUnique();
    }
}