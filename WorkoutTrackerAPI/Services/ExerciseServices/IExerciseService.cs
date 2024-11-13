﻿using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Services.ExerciseServices;

public interface IExerciseService
{
    Task<ServiceResult<Exercise>> GetInternalExerciseByIdAsync(long exerciseId);
    Task<ServiceResult<Exercise>> GetUserExerciseByIdAsync(string userId, long exerciseId);

    Task<ServiceResult<Exercise>> GetInternalExerciseByNameAsync(string name);
    Task<ServiceResult<Exercise>> GetUserExerciseByNameAsync(string userId, string name);

    Task<ServiceResult<IQueryable<Exercise>>> GetInternalExercisesAsync(ExerciseType? exerciseType = null);
    Task<ServiceResult<IQueryable<Exercise>>> GetUserExercisesAsync(string userId, ExerciseType? exerciseType = null);
    Task<ServiceResult<IQueryable<Exercise>>> GetAllExercisesAsync(string userId, ExerciseType? exerciseType = null);

    Task<ServiceResult<Exercise>> AddInternalExerciseAsync(Exercise model);
    Task<ServiceResult<Exercise>> AddUserExerciseAsync(string userId, Exercise model);

    Task<ServiceResult> UpdateInternalExerciseAsync(Exercise model);
    Task<ServiceResult> UpdateUserExerciseAsync(string userId, Exercise model);

    Task<ServiceResult> DeleteInternalExerciseAsync(long exerciseId);
    Task<ServiceResult> DeleteExerciseFromUserAsync(string userId, long exerciseId);

    Task<bool> InternalExerciseExistsAsync(long exerciseId);
    Task<bool> UserExerciseExistsAsync(string userId, long exerciseId);

    Task<bool> InternalExerciseExistsByNameAsync(string name);
    Task<bool> UserExerciseExistsByNameAsync(string userId, string name);
}
