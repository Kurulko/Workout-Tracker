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

        var userWorkouts = (await userRepository.GetUserWorkoutsAsync(userId))!;

        if (userWorkouts is null || !userWorkouts.Any())
            return new TotalUserProgress();

        int totalWorkouts = userWorkouts.Sum(w => w.CountOfTrainings);
        ModelWeight totalWeightLifted = userWorkouts.GetTotalWeightValue();
        TimeSpan totalDuration = userWorkouts.GetTotalTime();
        var workoutDates = userWorkouts.SelectMany(w => w.WorkoutRecords!.Select(wr => wr.Date)).ToList();
        DateTime? firstWorkoutDate = user.StartedWorkingOut;

        TimeSpan averageWorkoutDuration = TimeSpan.FromMinutes(totalDuration.TotalMinutes / totalWorkouts);
        int countOfDaysSinceFirstWorkout = firstWorkoutDate == null ? 0 : (int)(DateTime.Now - firstWorkoutDate.Value).TotalDays;

        const int daysInWeek = 7;
        double countOfWeeks = countOfDaysSinceFirstWorkout / daysInWeek;
        var frequencyPerWeek = (double)Math.Round((decimal)(totalWorkouts / countOfWeeks), 1);

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