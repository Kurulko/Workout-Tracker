using AutoMapper;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;
using WorkoutTracker.Domain.Entities.Muscles;

namespace WorkoutTracker.Application.Profiles.Muscles;

public class MuscleProfile : Profile
{
    public MuscleProfile()
    {
        CreateMap<Muscle, MuscleDTO>()
            .ForMember(
                dest => dest.ParentMuscleName,
                opt => opt.MapFrom(src => src.ParentMuscle != null ? src.ParentMuscle.Name : null)
            )
            .ForMember(
                dest => dest.ChildMuscles,
                opt => opt.MapFrom(src => src.ChildMuscles.GetModelsOrEmpty().Select(cm => new ChildMuscleDTO() { Id = cm.Id, Name = cm.Name }))
            )
            .ReverseMap();

        CreateMap<Muscle, MuscleDetailsDTO>()
           .ForMember(
               dest => dest.Muscle,
               opt => opt.MapFrom(src => src)
           )
           .ReverseMap();

        CreateMap<MuscleCreationDTO, Muscle>().ReverseMap();
        CreateMap<MuscleUpdateDTO, Muscle>().ReverseMap();
    }
}