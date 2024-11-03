using WorkoutTrackerAPI.Data;
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
    readonly NotFoundException bodyWeightNotFoundException = new("Body weight");


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

            BodyWeight? bodyWeight = await baseRepository.GetByIdAsync(bodyWeightId) ?? throw bodyWeightNotFoundException;
            
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

    public async Task<ServiceResult<IQueryable<BodyWeight>>> GetUserBodyWeightsAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId);
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

    public async Task<ServiceResult<BodyWeight>> GetMaxUserBodyWeightAsync(string userId)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId);
            var userMaxBodyWeight = userBodyWeights?.ToList().MaxBy(bw => BodyWeight.GetBodyWeightInKilos(bw));

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
            var userMinBodyWeight = userBodyWeights?.ToList().MinBy(bw => BodyWeight.GetBodyWeightInKilos(bw));
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

    public async Task<ServiceResult<BodyWeight>> GetUserBodyWeightByDateAsync(string userId, DateOnly date)
    {
        try
        {
            await CheckUserIdAsync(userRepository, userId);

            if (date > DateOnly.FromDateTime(DateTime.Now))
                throw new ArgumentException("Incorrect date.");

            var userBodyWeightByDate = (await baseRepository.FindAsync(bw => DateOnly.FromDateTime(bw.Date) == date && bw.UserId == userId)).FirstOrDefault();
            return ServiceResult<BodyWeight>.Ok(userBodyWeightByDate);
        }
        catch (Exception ex) when (ex is ArgumentException || ex is NotFoundException)
        {
            return ServiceResult<BodyWeight>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToActionStr("body weight by date", "get", ex));
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

            var _bodyWeight = await baseRepository.GetByIdAsync(bodyWeight.Id) ?? throw bodyWeightNotFoundException;
            
            if (_bodyWeight.UserId != userId)
                throw UserNotHavePermissionException("update", "body weight");

            await baseRepository.UpdateAsync(bodyWeight);
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
