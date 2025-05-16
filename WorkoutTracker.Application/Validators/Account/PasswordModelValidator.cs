using FluentValidation;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Account;

internal class PasswordModelValidator : AbstractValidator<PasswordModel>
{
    public PasswordModelValidator()
    {
        RuleFor(x => x.OldPassword)
          .ValidPassword("Please, enter your old password.");
        
        RuleFor(x => x.NewPassword)
          .ValidPassword("Please, enter your new password.");

        RuleFor(x => x.ConfirmNewPassword)
          .ValidPassword("Please, confirm new password.")
          .PasswordsMatch(x => x.NewPassword);
    }
}
