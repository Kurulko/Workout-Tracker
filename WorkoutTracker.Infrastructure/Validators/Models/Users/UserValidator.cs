using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Validators.Models.Base;

namespace WorkoutTracker.Infrastructure.Validators.Models.Users;

public class UserValidator
{
    readonly IUserRepository userRepository;
    public UserValidator(IUserRepository userRepository)
        => this.userRepository = userRepository;

    const string userEntryStr = "User";

    public async Task ValidateForAddAsync(User model)
    {
        await EnsureNonExistsAsync(model.Id);
        await ArgumentValidator.EnsureNonExistsByNameAsync(userRepository.GetUserByUsernameAsync, model.UserName!);

        Validate(model);
    }

    public async Task<User> ValidateForEditAsync(User model)
    {
        var user = await EnsureExistsAsync(model.Id);
        await EnsureNameUniqueAsync(model.UserName!, model.Id);

        Validate(model);

        return user;
    }

    public async Task<User> EnsureExistsAsync(string id)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(id, userEntryStr);

        return await ArgumentValidator.EnsureExistsByIdAsync(userRepository.GetUserByIdAsync, id, userEntryStr);
    }

    public async Task EnsureNonExistsAsync(string id)
    {
        ArgumentValidator.ThrowIfIdNullOrEmpty(id, userEntryStr);

        await ArgumentValidator.EnsureNonExistsByIdAsync(userRepository.GetUserByIdAsync, id);
    }

    async Task EnsureNameUniqueAsync(string name, string id)
    {
        ArgumentValidator.ThrowIfArgumentNullOrEmpty(name, nameof(User.UserName));

        var user = await userRepository.GetUserByUsernameAsync(name);

        if (user != null && user.Id != id)
            throw new ValidationException($"{nameof(User.UserName)} must be unique."); ;
    }

    void Validate(User model)
    {
        ArgumentValidator.ThrowIfDateInFuture(model.Registered, nameof(User.Registered));

        if (model.StartedWorkingOut.HasValue)
            ArgumentValidator.ThrowIfDateInFuture(model.StartedWorkingOut.Value, nameof(User.StartedWorkingOut));

        ArgumentValidator.ThrowIfValueNegative(model.CountOfTrainings, nameof(User.CountOfTrainings));
    }
}