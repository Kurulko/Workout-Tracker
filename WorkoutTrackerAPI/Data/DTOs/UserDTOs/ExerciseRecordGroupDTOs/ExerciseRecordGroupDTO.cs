using WorkoutTrackerAPI.Data.Enums;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Data.DTOs.UserDTOs;

public class ExerciseRecordGroupDTO
{
    public long Id { get; set; }

    public ModelWeight Weight { get; set; }
    public int Sets { get; set; }

    public long ExerciseId { get; set; }
    public string? ExerciseName { get; set; }
    public ExerciseType? ExerciseType { get; set; }

    public IEnumerable<ExerciseRecordDTO> ExerciseRecords { get; set; } = null!;
}
