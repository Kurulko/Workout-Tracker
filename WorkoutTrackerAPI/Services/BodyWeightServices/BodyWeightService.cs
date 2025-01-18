using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.BodyWeightServices;

namespace WorkoutTrackerAPI.Services;

public class BodyWeightService : DbModelService<BodyWeight>, IBodyWeightService
{
    readonly UserRepository userRepository;
    public BodyWeightService(BodyWeightRepository baseRepository, UserRepository userRepository) : base(baseRepository)
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

    async Task<ServiceResult<IQueryable<BodyWeight>>> GetUserBodyWeightsAsync(string userId, DateTime? date = null)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (date is DateTime _date && _date.Date > DateTime.Now.Date)
                throw new ArgumentException("Incorrect date.");

            var userBodyWeights = await baseRepository.FindAsync(ms => ms.UserId == userId);

            if (date.HasValue)
                userBodyWeights = userBodyWeights.Where(bw => bw.Date.Date == date.Value.Date);

            return ServiceResult<IQueryable<BodyWeight>>.Ok(userBodyWeights);
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

    public async Task<ServiceResult<IQueryable<BodyWeight>>> GetUserBodyWeightsInPoundsAsync(string userId, DateTime? date = null)
    {
        var serviceResult = await GetUserBodyWeightsAsync(userId, date);

        if (!serviceResult.Success)
            return serviceResult;

        var userBodyWeightsInPounds = serviceResult.Model!.AsEnumerable().Select(m =>
        {
            m.Weight = ModelWeight.GetModelWeightInPounds(m.Weight);
            return m;
        }).AsQueryable();

        return ServiceResult<IQueryable<BodyWeight>>.Ok(userBodyWeightsInPounds);
    }

    public async Task<ServiceResult<IQueryable<BodyWeight>>> GetUserBodyWeightsInKilogramsAsync(string userId, DateTime? date = null)
    {
        var serviceResult = await GetUserBodyWeightsAsync(userId, date);

        if (!serviceResult.Success)
            return serviceResult;

        var userBodyWeightsInKilograms = serviceResult.Model!.AsEnumerable().Select(m =>
        {
            m.Weight = ModelWeight.GetModelWeightInKilos(m.Weight);
            return m;
        }).AsQueryable();

        return ServiceResult<IQueryable<BodyWeight>>.Ok(userBodyWeightsInKilograms);
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
