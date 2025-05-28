using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Domain.Base;
using WorkoutTracker.Application.Interfaces.Repositories.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Base;

public abstract class DbModelValidator<T> : IModelValidator<T, long>
    where T : class, IDbModel
{
    protected readonly string modelEntryName;
    protected readonly IBaseRepository<T> baseRepository;

    protected DbModelValidator(string modelEntryName, IBaseRepository<T> baseRepository)
    {
        this.modelEntryName = modelEntryName;
        this.baseRepository = baseRepository;
    }

    public virtual async Task ValidateForAddAsync(T model, CancellationToken cancellationToken)
    {
        await EnsureNonExistsAsync(model.Id, cancellationToken);
        Validate(model);
    }

    public virtual async Task<T> ValidateForEditAsync(T model, CancellationToken cancellationToken)
    {
        var entity = await EnsureExistsAsync(model.Id, cancellationToken);
        Validate(model);

        return entity;
    }

    public abstract void Validate(T model);

    public virtual async Task<T> EnsureExistsAsync(long id, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonPositive(id, modelEntryName);

        return await ArgumentValidator.EnsureExistsByIdAsync(baseRepository.GetByIdAsync, id, modelEntryName, cancellationToken);
    }

    public virtual Task EnsureNonExistsAsync(long id, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfIdNonZero(id, modelEntryName);
        return Task.CompletedTask;
    }
}
