using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.DTOs.Account;

namespace WorkoutTracker.Infrastructure.Validators.Models.Auth;

public abstract class AccountModelValidator<T>
    where T : AccountModel
{
    public virtual void Validate(T model)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(model.Name, nameof(AccountModel.Name));
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(model.Password, nameof(AccountModel.Password));
    }
}
