using FluentValidation;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Account;

internal class RegisterModelValidator : AccountModelValidator<RegisterModel>
{
    public RegisterModelValidator()
    {
        RuleFor(x => x.Email)
          .ValidEmail("Please, enter your email.", false);

        RuleFor(x => x.PasswordConfirm)
          .ValidPassword("Please, confirm password.")
          .PasswordsMatch(x => x.Password);
    }
}
