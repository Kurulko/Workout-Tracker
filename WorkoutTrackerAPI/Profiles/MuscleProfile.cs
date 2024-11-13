using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Profiles;

public class MuscleProfile : Profile
{
    public MuscleProfile()
    {
        CreateMap<Muscle, MuscleDTO>()
            .ForMember(
                dest => dest.ParentMuscleName,
                opt => opt.MapFrom(src => src.ParentMuscle != null ? src.ParentMuscle.Name : null)
            )
            .ReverseMap();
    }
}