using Newtonsoft.Json;

namespace WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;

public class WorkoutDetailsDTO
{
    public WorkoutDTO Workout { get; set; } = null!;

    [JsonProperty("countOfWorkouts")]
    public int CountOfTrainings { get; set; }

    public ModelWeight SumOfWeight { get; set; }
    public TimeSpanModel SumOfTime { get; set; }

    public IEnumerable<MuscleDTO> Muscles { get; set; } = null!;
    public IEnumerable<EquipmentDTO> Equipments { get; set; } = null!;
}
