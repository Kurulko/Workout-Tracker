using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.DTOs.Account;

namespace WorkoutTracker.Infrastructure.Validators.Models.Auth;

public class RegisterModelValidator : AccountModelValidator<RegisterModel>
{
    public override void Validate(RegisterModel model)
    {
        base.Validate(model);

        ArgumentValidator.ThrowIfArgumentNullOrEmpty(model.Email, nameof(RegisterModel.Email));
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(model.PasswordConfirm, nameof(RegisterModel.PasswordConfirm));
        ArgumentValidator.ThrowIfPasswordsMismatch(model.Password, model.PasswordConfirm);
    }
}
