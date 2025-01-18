using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Profiles;

public class UserDetailsProfile : Profile
{
    public UserDetailsProfile()
    {
        CreateMap<UserDetailsDTO, UserDetails>().ReverseMap();
    }
}