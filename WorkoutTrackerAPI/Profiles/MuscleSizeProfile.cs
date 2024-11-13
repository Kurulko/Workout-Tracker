using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Profiles;

public class MuscleSizeProfile : Profile
{
    public MuscleSizeProfile()
    {
        CreateMap<MuscleSize, MuscleSizeDTO>()
            .ForMember(
                dest => dest.MuscleName,
                opt => opt.MapFrom(src => src.Muscle != null ? src.Muscle.Name : null)
            )
            .ReverseMap();

        CreateMap<MuscleSize, MuscleSizeCreationDTO>()
            .ReverseMap();
    }
}