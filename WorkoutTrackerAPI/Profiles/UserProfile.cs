using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Profiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<UserCreationDTO, User>()
            .ForMember(
                dest => dest.Id,
                opt => opt.MapFrom(src => 0)
            )
            .ForMember(
                dest => dest.UserName,
                opt => opt.MapFrom(src => src.UserName)
            )
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Email)
            )
            .ForMember(
                dest => dest.StartedWorkingOut,
                opt => opt.MapFrom(src => src.StartedWorkingOut)
            );

        CreateMap<User, UserDTO>()
            .ForMember(
                dest => dest.UserId,
                opt => opt.MapFrom(src => src.Id)
            )
            .ForMember(
                dest => dest.UserName,
                opt => opt.MapFrom(src => src.UserName)
            )
            .ForMember(
                dest => dest.Email,
                opt => opt.MapFrom(src => src.Email)
            )
            .ForMember(
                dest => dest.StartedWorkingOut,
                opt => opt.MapFrom(src => src.StartedWorkingOut)
            ) 
            .ForMember(
                dest => dest.Registered,
                opt => opt.MapFrom(src => src.Registered)
            );
    }
}