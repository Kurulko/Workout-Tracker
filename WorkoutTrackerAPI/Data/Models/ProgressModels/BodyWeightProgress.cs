namespace WorkoutTrackerAPI.Data.Models.ProgressModels;

public class BodyWeightProgress
{
    public ModelWeight AverageBodyWeight { get; set; }
    public ModelWeight MinBodyWeight { get; set; }
    public ModelWeight MaxBodyWeight { get; set; }
}
