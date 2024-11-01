using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Profiles;

public class ExerciseRecordProfile : Profile
{
    public ExerciseRecordProfile()
    {
        CreateMap<ExerciseRecordDTO, ExerciseRecord>().ReverseMap();
    }
}