using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;

namespace WorkoutTrackerAPI.Data;

public class WorkoutDbContext : IdentityDbContext<User>
{
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Muscle> Muscles => Set<Muscle>();
    public DbSet<Workout> Workouts => Set<Workout>();

    public WorkoutDbContext(DbContextOptions options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Exercise>()
            .HasOne(e => e.CreatedByUser)
            .WithMany(u => u.CreatedExercises)
            .HasForeignKey(e => e.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<ExerciseRecord>()
            .HasOne(er => er.User)
            .WithMany(u => u.ExerciseRecords)
            .HasForeignKey(er => er.UserId);

        modelBuilder.Entity<ExerciseRecord>()
            .HasOne(er => er.Exercise)
            .WithMany(e => e.ExerciseRecords)
            .HasForeignKey(er => er.ExerciseId);
    }

    public override int SaveChanges()
    {
        ValidateEntities();
        return base.SaveChanges();
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ValidateEntities();
        return await base.SaveChangesAsync(cancellationToken);
    }

    void ValidateEntities()
    {
        foreach (var entry in ChangeTracker.Entries<IDbModel>())
        {
            if (entry.State == EntityState.Added && entry.Entity.Id != 0)
                throw new DbUpdateException($"New entities of type {entry.Entity.GetType().Name} should not have an ID assigned.");

            if (entry.State == EntityState.Modified && entry.Entity.Id <= 0)
                throw new DbUpdateException($"Modified entities of type {entry.Entity.GetType().Name} must have a positive ID.");
        }
    }
}
