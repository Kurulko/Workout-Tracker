using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs.UserDTOs;

public class WorkoutRecordCreationDTO
{
    public TimeSpanModel Time { get; set; }
    public DateTime Date { get; set; }
    public long WorkoutId { get; set; }
    public IEnumerable<ExerciseRecordGroupCreationDTO> ExerciseRecordGroups { get; set; } = null!;
}
