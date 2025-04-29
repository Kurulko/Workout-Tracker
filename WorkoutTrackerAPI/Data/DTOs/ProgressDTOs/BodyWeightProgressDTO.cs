namespace WorkoutTrackerAPI.Data.DTOs.ProgressDTOs;

public class BodyWeightProgressDTO
{
    public ModelWeight AverageBodyWeight { get; set; }
    public ModelWeight MinBodyWeight { get; set; }
    public ModelWeight MaxBodyWeight { get; set; }
}
