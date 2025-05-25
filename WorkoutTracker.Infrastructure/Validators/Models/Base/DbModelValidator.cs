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

    public virtual async Task ValidateForAddAsync(T model)
    {
        await EnsureNonExistsAsync(model.Id);
        Validate(model);
    }

    public virtual async Task<T> ValidateForEditAsync(T model)
    {
        var entity = await EnsureExistsAsync(model.Id);
        Validate(model);

        return entity;
    }

    public abstract void Validate(T model);

    public virtual async Task<T> EnsureExistsAsync(long id)
    {
        ArgumentValidator.ThrowIfIdNonPositive(id, modelEntryName);

        return await ArgumentValidator.EnsureExistsByIdAsync(baseRepository.GetByIdAsync, id, modelEntryName);
    }

    public virtual Task EnsureNonExistsAsync(long id)
    {
        ArgumentValidator.ThrowIfIdNonZero(id, modelEntryName);
        return Task.CompletedTask;
    }
}
