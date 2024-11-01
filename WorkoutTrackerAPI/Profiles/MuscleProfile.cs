using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Profiles;

public class MuscleProfile : Profile
{
    public MuscleProfile()
    {
        CreateMap<MuscleDTO, Muscle>().ReverseMap();
    }
}