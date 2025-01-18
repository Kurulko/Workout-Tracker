using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.MuscleDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Extentions;

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