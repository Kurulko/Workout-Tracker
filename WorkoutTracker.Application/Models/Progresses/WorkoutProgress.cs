namespace WorkoutTracker.Application.Models.Progresses;

public class WorkoutProgress
{
    public BaseInfoProgress BaseInfoProgress { get; set; } = null!;
    public TotalCompletedProgress TotalCompletedProgress { get; set; } = null!;
    public WorkoutWeightLiftedProgress WorkoutWeightLiftedProgress { get; set; } = null!;
    public StrikeDurationProgress StrikeDurationProgress { get; set; } = null!;
    public WorkoutDurationProgress WorkoutDurationProgress { get; set; } = null!;
    public BodyWeightProgress BodyWeightProgress { get; set; } = null!;
}