using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Users;

public class UserDetailsValidator : DbModelValidator<UserDetails>
{
    public UserDetailsValidator(IUserDetailsRepository userDetailsRepository)
        : base("User details", userDetailsRepository)
    {
    }

    public override void Validate(UserDetails model)
    {
        if(model.DateOfBirth.HasValue)
            ArgumentValidator.ThrowIfDateInFuture(model.DateOfBirth.Value, nameof(UserDetails.DateOfBirth));

        if (model.Weight.HasValue)
            ArgumentValidator.ThrowIfModelWeightNegative(model.Weight.Value, nameof(UserDetails.Weight));

        if (model.Height.HasValue)
            ArgumentValidator.ThrowIfModelSizeNegative(model.Height.Value, nameof(UserDetails.Height));

        if (model.BodyFatPercentage.HasValue)
        {
            const int minBodyFatPercentage = 0, maxBodyFatPercentage = 100;
            ArgumentValidator.ThrowIfOutOfRange(minBodyFatPercentage..maxBodyFatPercentage, model.BodyFatPercentage.Value, nameof(UserDetails.BodyFatPercentage));
        }
    }
}