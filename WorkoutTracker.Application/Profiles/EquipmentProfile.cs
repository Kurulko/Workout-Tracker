using AutoMapper;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.DTOs.Equipments;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Profiles;

public class EquipmentProfile : Profile
{
    public EquipmentProfile()
    {
        CreateMap<Equipment, EquipmentDTO>()
            .ForMember(
                dest => dest.IsOwnedByUser,
                opt => opt.MapFrom(src => src.OwnedByUserId != null)
            )
            .ReverseMap();


        CreateMap<Equipment, EquipmentDetailsDTO>()
            .ForMember(
                dest => dest.Equipment,
                opt => opt.MapFrom(src => src)
            )
            .ForMember(
                dest => dest.Muscles,
                opt => opt.MapFrom(src => src.Exercises.GetModelsOrEmpty().SelectMany(e => e.WorkingMuscles).DistinctBy(m => m.Id).Select(m => new ChildMuscleDTO() { Id = m.Id, Name = m.Name }))
            )
            .ReverseMap();

        CreateMap<EquipmentCreationDTO, Equipment>().ReverseMap();
        CreateMap<EquipmentUpdateDTO, Equipment>().ReverseMap();
    }
}