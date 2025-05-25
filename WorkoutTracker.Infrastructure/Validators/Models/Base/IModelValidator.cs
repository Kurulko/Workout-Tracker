namespace WorkoutTracker.Infrastructure.Validators.Models.Base;

public interface IModelValidator<T, T_ID>
    where T : class
{
    Task ValidateForAddAsync(T model);
    Task<T> ValidateForEditAsync(T model);

    Task<T> EnsureExistsAsync(T_ID id);
    Task EnsureNonExistsAsync(T_ID id);
}
