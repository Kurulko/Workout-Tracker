using AutoMapper;
using Microsoft.AspNetCore.Identity;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;

namespace WorkoutTrackerAPI.Profiles;

public class RoleProfile : Profile
{
    public RoleProfile()
    {
        CreateMap<RoleDTO, IdentityRole>().ReverseMap();

        CreateMap<RoleCreationDTO, IdentityRole>()
           .ForMember(
               dest => dest.Id,
               opt => opt.MapFrom(src => Guid.NewGuid().ToString())
           )
           .ReverseMap();
    }
}