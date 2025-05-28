using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories.Base;
using WorkoutTracker.Domain.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Base;

public abstract class BaseWorkoutModelValidator<T> : DbModelValidator<T>
    where T : BaseWorkoutModel
{
    protected readonly IBaseWorkoutRepository<T> baseWorkoutRepository;
    protected BaseWorkoutModelValidator(string modelEntryName, IBaseWorkoutRepository<T> baseWorkoutRepository)
        : base(modelEntryName, baseWorkoutRepository)
    {
        this.baseWorkoutRepository = baseWorkoutRepository;
    }

    public override async Task ValidateForAddAsync(T model, CancellationToken cancellationToken)
    {
        await EnsureNonExistsAsync(model.Id, cancellationToken);
        await ArgumentValidator.EnsureNonExistsByNameAsync(baseWorkoutRepository.GetByNameAsync, model.Name, cancellationToken);

        Validate(model);
    }

    public override async Task<T> ValidateForEditAsync(T model, CancellationToken cancellationToken)
    {
        var entity = await EnsureExistsAsync(model.Id, cancellationToken);
        await EnsureNameUniqueAsync(model.Name, model.Id, cancellationToken);

        Validate(model);

        return entity;
    }

    public virtual async Task EnsureNameUniqueAsync(string name, long id, CancellationToken cancellationToken)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(BaseWorkoutModel.Name));

        await ArgumentValidator.EnsureNameUniqueAsync(baseWorkoutRepository.GetByNameAsync, name, id, nameof(BaseWorkoutModel.Name), cancellationToken);
    }
}
