using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Profiles;

public class BodyWeightProfile : Profile
{
    public BodyWeightProfile()
    {
        CreateMap<BodyWeightDTO, BodyWeight>().ReverseMap();
    }
}