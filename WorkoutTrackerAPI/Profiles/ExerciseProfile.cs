using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Profiles;

public class ExerciseProfile : Profile
{
    public ExerciseProfile()
    {
        CreateMap<Exercise, ExerciseDTO>()
            .ForMember(
                dest => dest.IsCreatedByUser,
                opt => opt.MapFrom(src => src.CreatedByUserId != null)
            )
            .ReverseMap();
    }
}