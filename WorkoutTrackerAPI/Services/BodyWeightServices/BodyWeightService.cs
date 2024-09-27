using Microsoft.EntityFrameworkCore.Metadata.Internal;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Repositories;
using WorkoutTrackerAPI.Repositories.UserRepositories;
using WorkoutTrackerAPI.Services.BodyWeightServices;

namespace WorkoutTrackerAPI.Services;

public class BodyWeightService : Service<BodyWeight>, IBodyWeightService
{
    readonly UserRepository userRepository;
    public BodyWeightService(BodyWeightRepository baseRepository, UserRepository userRepository) : base(baseRepository)
        => this.userRepository = userRepository;

    readonly EntryNullException bodyWeightIsNullException = new ("Body weight");
    readonly InvalidIDException invalidBodyWeightIDException = new (nameof(BodyWeight));
    readonly NotFoundException bodyWeightNotFoundException = new ("Body weight");


    public async Task<ServiceResult<BodyWeight>> AddBodyWeightToUserAsync(string userId, BodyWeight bodyWeight)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<BodyWeight>.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<BodyWeight>.Fail(userNotFoundException);

        if (bodyWeight is null)
            return ServiceResult<BodyWeight>.Fail(bodyWeightIsNullException);

        if (bodyWeight.Id != 0)
            return ServiceResult<BodyWeight>.Fail(InvalidEntryIDWhileAddingStr(nameof(BodyWeight), "body weight"));

        try
        {
            bodyWeight.UserId = userId;
            await baseRepository.AddAsync(bodyWeight);

            return ServiceResult<BodyWeight>.Ok(bodyWeight);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToAction("body weight", "add", ex.Message));
        }
    }

    public async Task<ServiceResult> DeleteBodyWeightFromUserAsync(string userId, long bodyWeightId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (bodyWeightId < 1)
            return ServiceResult.Fail(invalidBodyWeightIDException);

        BodyWeight? bodyWeight = await baseRepository.GetByIdAsync(bodyWeightId);

        if (bodyWeight is null)
            return ServiceResult.Fail(bodyWeightNotFoundException);

        if (bodyWeight.UserId != userId)
            return ServiceResult.Fail(UserNotHavePermissionStr("delete", "body weight"));

        try
        {
            await baseRepository.RemoveAsync(bodyWeightId);
            return ServiceResult.Ok();
        }
        catch
        {
            return ServiceResult.Fail(FailedToAction("body weight", "delete"));
        }
    }

    public async Task<ServiceResult<IQueryable<BodyWeight>>> GetUserBodyWeightsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<IQueryable<BodyWeight>>.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<IQueryable<BodyWeight>>.Fail(userNotFoundException);

        try
        {
            var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId);
            return ServiceResult<IQueryable<BodyWeight>>.Ok(userBodyWeights);
        }
        catch (Exception ex)
        {
            return ServiceResult<IQueryable<BodyWeight>>.Fail(FailedToAction("body weights", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<BodyWeight>> GetMaxUserBodyWeightAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<BodyWeight>.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<BodyWeight>.Fail(userNotFoundException);

        try
        {
            var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId);
            var userMaxBodyWeight = userBodyWeights?.MaxBy(bw => BodyWeight.GetBodyWeightInKilos(bw));
            return ServiceResult<BodyWeight>.Ok(userMaxBodyWeight);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToAction("max body weight", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<BodyWeight>> GetMinUserBodyWeightAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<BodyWeight>.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<BodyWeight>.Fail(userNotFoundException);

        try
        {
            var userBodyWeights = await baseRepository.FindAsync(bw => bw.UserId == userId);
            var userMinBodyWeight = userBodyWeights?.MinBy(bw => BodyWeight.GetBodyWeightInKilos(bw));
            return ServiceResult<BodyWeight>.Ok(userMinBodyWeight);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToAction("min body weight", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<BodyWeight>> GetUserBodyWeightByDateAsync(string userId, DateOnly date)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<BodyWeight>.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<BodyWeight>.Fail(userNotFoundException);

        if (date > DateOnly.FromDateTime(DateTime.Now))
            return ServiceResult<BodyWeight>.Fail("Incorrect date.");

        try
        {
            var userBodyWeightByDate = (await baseRepository.FindAsync(bw => bw.Date == date))?.First();
            return ServiceResult<BodyWeight>.Ok(userBodyWeightByDate);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToAction("body weight by date", "get", ex.Message));
        }
    }

    public async Task<ServiceResult<BodyWeight>> GetUserBodyWeightByIdAsync(string userId, long bodyWeightId)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult<BodyWeight>.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult<BodyWeight>.Fail(userNotFoundException);

        if (bodyWeightId < 1)
            return ServiceResult<BodyWeight>.Fail(invalidBodyWeightIDException);

        try
        {
            var userBodyWeightById = await baseRepository.GetByIdAsync(bodyWeightId);
            return ServiceResult<BodyWeight>.Ok(userBodyWeightById);
        }
        catch (Exception ex)
        {
            return ServiceResult<BodyWeight>.Fail(FailedToAction("body weight", "get", ex.Message));
        }
    }

    public async Task<ServiceResult> UpdateUserBodyWeightAsync(string userId, BodyWeight bodyWeight)
    {
        if (string.IsNullOrEmpty(userId))
            return ServiceResult.Fail(userIdIsNullOrEmptyException);

       if (!(await userRepository.UserExistsAsync(userId)))
            return ServiceResult.Fail(userNotFoundException);

        if (bodyWeight is null)
            return ServiceResult.Fail(new EntryNullException(nameof(BodyWeight)));

        if (bodyWeight.Id < 1)
            return ServiceResult.Fail(invalidBodyWeightIDException);

        try
        {
            BodyWeight? _bodyWeight = await baseRepository.GetByIdAsync(bodyWeight.Id);

            if (_bodyWeight is null)
                return ServiceResult.Fail(bodyWeightNotFoundException);

            if (_bodyWeight.UserId != userId)
                return ServiceResult.Fail(UserNotHavePermissionStr("update", "body weight"));

            await baseRepository.UpdateAsync(bodyWeight);

            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            return ServiceResult.Fail(FailedToAction("body weight", "update", ex.Message));
        }
    }
}
