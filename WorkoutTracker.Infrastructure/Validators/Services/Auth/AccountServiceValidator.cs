using Microsoft.AspNetCore.Http;
using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Validators;
using WorkoutTracker.Application.DTOs.Account;
using WorkoutTracker.Infrastructure.Identity.Entities;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Validators.Models.Auth;
using WorkoutTracker.Infrastructure.Validators.Models.Users;

namespace WorkoutTracker.Infrastructure.Validators.Services.Auth;

public class AccountServiceValidator
{
    readonly UserValidator userValidator;
    readonly LoginModelValidator loginModelValidator;
    readonly RegisterModelValidator registerModelValidator;
    readonly IUserRepository userRepository;
    readonly IHttpContextAccessor httpContextAccessor;

    public AccountServiceValidator(
        UserValidator userValidator,
        LoginModelValidator loginModelValidator,
        RegisterModelValidator registerModelValidator,
        IUserRepository userRepository,
        IHttpContextAccessor httpContextAccessor
    )
    {
        this.userValidator = userValidator;
        this.loginModelValidator = loginModelValidator;
        this.registerModelValidator = registerModelValidator;

        this.userRepository = userRepository;
        this.httpContextAccessor = httpContextAccessor;
    }

    public Task ValidateLoginAsync(LoginModel login)
    {
        loginModelValidator.Validate(login);
        return Task.CompletedTask;
    }

    public async Task ValidateRegisterAsync(RegisterModel register)
    {
        registerModelValidator.Validate(register);

        await ArgumentValidator.EnsureNonExistsByNameAsync(userRepository.GetUserByUsernameAsync, register.Name);
        await ArgumentValidator.EnsureNonExistsByEmailAsync(userRepository.GetUserByEmailAsync, register.Email);
    }

    public Task ValidateLogoutAsync()
    {
        ThrowIfNotAuthenticated();
        return Task.CompletedTask;
    }

    public async Task ValidateGetTokenAsync(string userId)
    {
        ThrowIfNotAuthenticated();
        ArgumentValidator.ThrowIfIdNullOrEmpty(userId, nameof(User));
        await userValidator.EnsureExistsAsync(userId);
    }

    void ThrowIfNotAuthenticated()
    {
        if (httpContextAccessor.HttpContext?.User?.Identity is null || !httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            throw UnauthorizedException.UserNotAuthenticated();
    }
}