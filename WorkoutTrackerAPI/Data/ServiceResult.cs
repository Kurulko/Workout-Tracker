using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data;

public class ServiceResult
{
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private ServiceResult(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static ServiceResult Ok()
        => new ServiceResult(true);

    public static ServiceResult Fail(string errorMessage)
        => new ServiceResult(false, errorMessage);

    public static ServiceResult Fail(Exception ex)
        => Fail(ex.InnerException?.Message ?? ex.Message);
}

public class ServiceResult<T> where T : class
{
    public bool Success { get; }
    public T? Model { get; }
    public string? ErrorMessage { get; }

    private ServiceResult(bool success, T? model = null, string? errorMessage = null)
    {
        Success = success;
        Model = model;
        ErrorMessage = errorMessage;
    }

    public static ServiceResult<T> Ok(T? model)
        => new ServiceResult<T>(true, model);

    public static ServiceResult<T> Fail(string errorMessage)
        => new ServiceResult<T>(false, null, errorMessage);

    public static ServiceResult<T> Fail(Exception ex)
        => Fail(ex.InnerException?.Message ?? ex.Message);
}

