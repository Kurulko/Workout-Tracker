using FluentValidation;
using WorkoutTracker.Application.DTOs.Users;
using WorkoutTracker.Application.Extensions;

namespace WorkoutTracker.Application.Validators.Users;

internal class UserUpdateDTOValidator : AbstractValidator<UserUpdateDTO>
{
    public UserUpdateDTOValidator()
    {
        RuleFor(m => m.UserId)
            .NotEmptyOrNull("UserId is required.");

        RuleFor(m => m.UserName)
            .ValidName("Username is required.");

        RuleFor(m => m.Email)
            .ValidEmail(isRequired: false);

        RuleFor(m => m.Registered)
            .DateNotInFuture();

        RuleFor(m => m.StartedWorkingOut)
            .DateNotInFuture(isRequired: false);

    }
}
