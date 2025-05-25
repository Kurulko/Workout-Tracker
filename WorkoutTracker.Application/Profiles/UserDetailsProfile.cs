using AutoMapper;
using WorkoutTracker.Application.DTOs.Users;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Profiles;

public class UserDetailsProfile : Profile
{
    public UserDetailsProfile()
    {
        CreateMap<UserDetailsDTO, UserDetails>().ReverseMap();
    }
}