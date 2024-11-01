using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Profiles;

public class MuscleSizeProfile : Profile
{
    public MuscleSizeProfile()
    {
        CreateMap<MuscleSizeDTO, MuscleSize>().ReverseMap();
    }
}