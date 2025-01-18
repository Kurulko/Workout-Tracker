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
            .ForMember(
                dest => dest.MusclePhoto,
                opt => opt.MapFrom(src => src.Muscle != null ? src.Muscle.Image : null)
            )
            .ReverseMap();

        CreateMap<MuscleSize, MuscleSizeCreationDTO>()
            .ReverseMap();
    }
}