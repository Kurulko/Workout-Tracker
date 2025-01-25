using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.EquipmentDTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.ExerciseDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Profiles;

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