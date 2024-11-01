using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

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
    }
}