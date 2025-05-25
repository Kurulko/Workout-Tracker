using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Persistence.Configurations.Base;

internal abstract class DbModelConfiguration<T> : IEntityTypeConfiguration<T>
    where T : class, IDbModel
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(m => m.Id);
    }
}
