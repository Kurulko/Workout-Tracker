using WorkoutTracker.Application.Common.Exceptions;
using WorkoutTracker.Application.Common.Models;
using WorkoutTracker.Application.Common.Results;
using WorkoutTracker.Application.Interfaces.Repositories;
using WorkoutTracker.Application.Interfaces.Services;
using WorkoutTracker.Domain.Entities;
using WorkoutTracker.Domain.ValueObjects;
using WorkoutTracker.Infrastructure.Exceptions;
using WorkoutTracker.Infrastructure.Identity.Interfaces.Repositories;
using WorkoutTracker.Infrastructure.Services.Base;
using WorkoutTracker.Application.Common.Extensions;

namespace WorkoutTracker.Infrastructure.Services;

internal class BodyWeightService : DbModelService<BodyWeight>, IBodyWeightService
{
    readonly IUserRepository userRepository;
    public BodyWeightService(IBodyWeightRepository baseRepository, IUserRepository userRepository) : base(baseRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException bodyWeightIsNullException = new("Body weight");
    readonly InvalidIDException invalidBodyWeightIDException = new(nameof(BodyWeight));

    NotFoundException BodyWeightNotFoundByIDException(long id)
        => NotFoundException.NotFoundExceptionByID("Body weight", id);

    public async Task<ServiceResult<BodyWeight>> AddBodyWeightToUserAsync(string userId, BodyWeight bodyWeight)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (bodyWeight is null)
                throw bodyWeightIsNullException;

            if (bodyWeight.Id != 0)
                throw InvalidEntryIDWhileAddingException(nameof(BodyWeight), "body weight");

            bodyWeight.UserId = userId;
            await baseRepository.AddAsync(bodyWeight);

            return ServiceResult<BodyWeight>.Ok(bodyWeight);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<BodyWeight>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToActionStr("body weight", "add", ex));
        }
    }

    public async Task<ServiceResult> DeleteBodyWeightFromUserAsync(string userId, long bodyWeightId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (bodyWeightId < 1)
                throw invalidBodyWeightIDException;

            BodyWeight? bodyWeight = await baseRepository.GetByIdAsync(bodyWeightId) ?? throw BodyWeightNotFoundByIDException(bodyWeightId);
            
            if (bodyWeight.UserId != userId)
                throw UserNotHavePermissionException("delete", "body weight");

            await baseRepository.RemoveAsync(bodyWeightId);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch
        {
            return ServiceResult.Fail(FailedToActionStr("body weight", "delete"));
        }
    }

    async Task<ServiceResult<IQueryable<BodyWeight>>> GetUserBodyWeightsAsync(string userId, DateTimeRange? range = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (range is DateTimeRange _range && _range.LastDate > DateTime.Now.Date)
                throw new ArgumentException("Incorrect date.");

            IEnumerable<BodyWeight> userBodyWeights = (await baseRepository.FindAsync(wr => wr.UserId == userId)).ToList();

            if (range is not null)
                userBodyWeights = userBodyWeights.Where(bw => range.IsDateInRange(bw.Date, true));

            return ServiceResult<IQueryable<BodyWeight>>.Ok(userBodyWeights.AsQueryable());
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<IQueryable<BodyWeight>>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<BodyWeight>>.Fail(FailedToActionStr("body weights", "get", ex));
        }
    }

    public async Task<ServiceResult<IQueryable<BodyWeight>>> GetUserBodyWeightsInPoundsAsync(string userId, DateTimeRange? range = null)
    {
        var serviceResult = await GetUserBodyWeightsAsync(userId, range);

        if (!serviceResult.Success)
            return serviceResult;

        var userBodyWeightsInPounds = serviceResult.Model!.ToList().Select(m =>
        {
            m.Weight = ModelWeight.GetModelWeightInPounds(m.Weight);
            return m;
        }).AsQueryable();

        return ServiceResult<IQueryable<BodyWeight>>.Ok(userBodyWeightsInPounds);
    }

    public async Task<ServiceResult<IQueryable<BodyWeight>>> GetUserBodyWeightsInKilogramsAsync(string userId, DateTimeRange? range = null)
    {
        var serviceResult = await GetUserBodyWeightsAsync(userId, range);

        if (!serviceResult.Success)
            return serviceResult;

        var userBodyWeightsInKilograms = serviceResult.Model!.ToList().Select(m =>
        {
            m.Weight = ModelWeight.GetModelWeightInKilos(m.Weight);
            return m;
        }).AsQueryable();

        return ServiceResult<IQueryable<BodyWeight>>.Ok(userBodyWeightsInKilograms);
    }

    public async Task<ServiceResult<BodyWeight>> GetCurrentUserBodyWeightAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId);
            var userMaxBodyWeight = userBodyWeights?.ToList().MaxBy(bw => bw.Date);

            return ServiceResult<BodyWeight>.Ok(userMaxBodyWeight);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<BodyWeight>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToActionStr("current body weight", "get", ex));
        }
    }

    public async Task<ServiceResult<BodyWeight>> GetMaxUserBodyWeightAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId);
            var userMaxBodyWeight = userBodyWeights?.ToList().MaxBy(bw => bw.Weight);

            return ServiceResult<BodyWeight>.Ok(userMaxBodyWeight);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<BodyWeight>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToActionStr("max body weight", "get", ex));
        }
    }

    public async Task<ServiceResult<BodyWeight>> GetMinUserBodyWeightAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId);
            var userMinBodyWeight = userBodyWeights?.ToList().MinBy(bw => bw.Weight);
            return ServiceResult<BodyWeight>.Ok(userMinBodyWeight);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<BodyWeight>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToActionStr("min body weight", "get", ex));
        }
    }

    public async Task<ServiceResult<BodyWeight>> GetUserBodyWeightByIdAsync(string userId, long bodyWeightId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (bodyWeightId < 1)
                throw invalidBodyWeightIDException;

            var userBodyWeightById = await baseRepository.GetByIdAsync(bodyWeightId);
            return ServiceResult<BodyWeight>.Ok(userBodyWeightById);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<BodyWeight>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToActionStr("body weight", "get", ex));
        }
    }

    public async Task<ServiceResult> UpdateUserBodyWeightAsync(string userId, BodyWeight bodyWeight)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (bodyWeight is null)
                throw bodyWeightIsNullException;

            if (bodyWeight.Id < 1)
                throw invalidBodyWeightIDException;

            var _bodyWeight = await baseRepository.GetByIdAsync(bodyWeight.Id) ?? throw BodyWeightNotFoundByIDException(bodyWeight.Id);
            
            if (_bodyWeight.UserId != userId)
                throw UserNotHavePermissionException("update", "body weight");

            _bodyWeight.Weight = bodyWeight.Weight;
            _bodyWeight.Date = bodyWeight.Date;

            await baseRepository.UpdateAsync(_bodyWeight);
            return ServiceResult.Ok();
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException || ex is UnauthorizedAccessException)
        {
            return ServiceResult.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToActionStr("body weight", "update", ex));
        }
    }
}
