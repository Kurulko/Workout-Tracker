using AutoMapper;
using Microsoft.AspNetCore.Identity;
using WorkoutTracker.Application.DTOs.Roles;

namespace WorkoutTracker.Infrastructure.Identity.Profiles;

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