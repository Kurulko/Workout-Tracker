namespace WorkoutTracker.Infrastructure.Validators.Models.Base;

public interface IModelValidator<T, T_ID>
    where T : class
{
    Task ValidateForAddAsync(T model, CancellationToken cancellationToken);
    Task<T> ValidateForEditAsync(T model, CancellationToken cancellationToken);

    Task<T> EnsureExistsAsync(T_ID id, CancellationToken cancellationToken);
    Task EnsureNonExistsAsync(T_ID id, CancellationToken cancellationToken);
}
