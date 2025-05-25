using FluentValidation;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Account;

internal abstract class AccountModelValidator<T> : AbstractValidator<T> 
    where T : AccountModel
{
    protected AccountModelValidator()
    {
        RuleFor(x => x.Name)
            .ValidName("Please, enter your name.");

        RuleFor(x => x.Password)
            .ValidPassword("Please, enter your password.");
    }
}
