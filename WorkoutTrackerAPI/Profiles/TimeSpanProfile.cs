using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;

namespace WorkoutTrackerAPI.Profiles;

public class TimeSpanProfile : Profile
{
    public TimeSpanProfile()
    {
        CreateMap<TimeSpan, TimeSpanModel>()
            .ConvertUsing(src => (TimeSpanModel)src);

        CreateMap<TimeSpanModel, TimeSpan>()
            .ConvertUsing(src => (TimeSpan)src);
    }
}