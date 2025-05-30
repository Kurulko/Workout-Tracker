using WorkoutTracker.Application.DTOs.Account;

namespace WorkoutTracker.Infrastructure.Validators.Models.Auth;

public class LoginModelValidator : AccountModelValidator<LoginModel>
{
    public override void Validate(LoginModel model)
    {
        base.Validate(model);
    }
}
