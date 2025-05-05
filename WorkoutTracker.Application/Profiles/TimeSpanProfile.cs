using AutoMapper;
using WorkoutTracker.Application.DTOs;

namespace WorkoutTracker.Application.Profiles;

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