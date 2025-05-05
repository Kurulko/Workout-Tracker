using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using WorkoutTracker.Infrastructure.Identity.Entities;
using AutoMapper;
using WorkoutTracker.Application.DTOs.Users;

namespace WorkoutTracker.Infrastructure.Identity.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCreationDTO, User>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => Guid.NewGuid().ToString())
            )
            .ReverseMap();

        CreateMap<User, UserCreationDTO>()
            .ReverseMap();

        CreateMap<UserDTO, User>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => src.UserId)
            )
            .ReverseMap();
    }
}