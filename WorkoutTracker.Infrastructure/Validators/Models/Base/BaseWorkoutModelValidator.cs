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

    public override async Task ValidateForAddAsync(T model)
    {
        await EnsureNonExistsAsync(model.Id);
        await ArgumentValidator.EnsureNonExistsByNameAsync(baseWorkoutRepository.GetByNameAsync, model.Name);

        Validate(model);
    }

    public override async Task<T> ValidateForEditAsync(T model)
    {
        var entity = await EnsureExistsAsync(model.Id);
        await EnsureNameUniqueAsync(model.Name, model.Id);

        Validate(model);

        return entity;
    }

    public virtual async Task EnsureNameUniqueAsync(string name, long id)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(BaseWorkoutModel.Name));

        await ArgumentValidator.EnsureNameUniqueAsync(baseWorkoutRepository.GetByNameAsync, name, id, nameof(BaseWorkoutModel.Name));
    }
}
