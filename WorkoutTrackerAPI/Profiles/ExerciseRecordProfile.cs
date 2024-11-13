using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Profiles;

public class ExerciseRecordProfile : Profile
{
    public ExerciseRecordProfile()
    {
        CreateMap<ExerciseRecord, ExerciseRecordDTO>()
            .ForMember(
                dest => dest.ExerciseName,
                opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Name : null)
            )
            .ReverseMap();

        CreateMap<ExerciseRecordCreationDTO, ExerciseRecord>().ReverseMap();
    }
}