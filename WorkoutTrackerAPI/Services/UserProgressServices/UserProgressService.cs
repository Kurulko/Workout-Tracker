using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Exceptions;
using WorkoutTrackerAPI.Extentions;
using WorkoutTrackerAPI.Repositories.UserRepositories;

namespace WorkoutTrackerAPI.Services.ProgressServices;

public class UserProgressService : IUserProgressService
{ 
    readonly UserRepository userRepository;
    public UserProgressService(UserRepository userRepository)
        => this.userRepository = userRepository;

    public async Task<TotalUserProgress> CalculateUserProgressAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            throw new ArgumentNullOrEmptyException("User ID");

        var user = await userRepository.GetUserByIdAsync(userId) ?? throw NotFoundException.NotFoundExceptionByID(nameof(User), userId);

        var userWorkoutRecords = (await userRepository.GetUserWorkoutRecordsAsync(userId))!;

        if (userWorkoutRecords is null || !userWorkoutRecords.Any())
            return new TotalUserProgress();

        int totalWorkouts = userWorkoutRecords.Count();
        ModelWeight totalWeightLifted = userWorkoutRecords.GetTotalWeightValue();
        TimeSpan totalDuration = userWorkoutRecords.GetTotalTime();
        var workoutDates = userWorkoutRecords.Select(wr => wr.Date).Distinct().ToList();
        DateTime? firstWorkoutDate = user.StartedWorkingOut;

        TimeSpan averageWorkoutDuration = TimeSpan.FromMinutes(totalDuration.TotalMinutes / totalWorkouts);
        int countOfDaysSinceFirstWorkout = firstWorkoutDate == null ? 0 : (int)(DateTime.Now - firstWorkoutDate.Value).TotalDays;

        const int daysInWeek = 7;
        double countOfWeeks = Math.Round((double)countOfDaysSinceFirstWorkout / daysInWeek) + 1;
        var frequencyPerWeek = Math.Round(totalWorkouts / countOfWeeks, 1);

        return new TotalUserProgress
        {
            TotalWorkouts = totalWorkouts,
            TotalWeightLifted = totalWeightLifted,
            TotalDuration = totalDuration!,
            FirstWorkoutDate = firstWorkoutDate,
            AverageWorkoutDuration = averageWorkoutDuration,
            CountOfDaysSinceFirstWorkout = countOfDaysSinceFirstWorkout,
            FrequencyPerWeek = frequencyPerWeek,
            WorkoutDates = workoutDates
        };
    }
}