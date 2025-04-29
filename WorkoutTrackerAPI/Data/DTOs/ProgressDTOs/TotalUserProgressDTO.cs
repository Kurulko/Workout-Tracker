namespace WorkoutTrackerAPI.Data.DTOs.ProgressDTOs;

public class WorkoutProgressDTO
{
    public BaseInfoProgressDTO BaseInfoProgress { get; set; } = null!;
    public TotalCompletedProgressDTO TotalCompletedProgress { get; set; } = null!;
    public WorkoutWeightLiftedProgressDTO WorkoutWeightLiftedProgress { get; set; } = null!;
    public StrikeDurationProgressDTO StrikeDurationProgress { get; set; } = null!;
    public WorkoutDurationProgressDTO WorkoutDurationProgress { get; set; } = null!;
    public BodyWeightProgressDTO BodyWeightProgress { get; set; } = null!;
}