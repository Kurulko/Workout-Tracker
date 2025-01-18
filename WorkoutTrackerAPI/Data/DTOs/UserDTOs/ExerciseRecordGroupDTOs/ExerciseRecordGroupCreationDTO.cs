using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Data.DTOs.UserDTOs;

public class ExerciseRecordGroupCreationDTO
{
    public long ExerciseId { get; set; }
    public IEnumerable<ExerciseRecordCreationDTO> ExerciseRecords { get; set; } = null!;
}
