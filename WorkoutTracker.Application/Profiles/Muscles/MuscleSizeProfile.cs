using AutoMapper;
using WorkoutTracker.Application.DTOs.Muscles.MuscleSizes;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Profiles.Muscles;

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