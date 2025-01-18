using AutoMapper;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Profiles;

public class TotalUserProgressProfile : Profile
{
    public TotalUserProgressProfile()
    {
        CreateMap<TotalUserProgress, TotalUserProgressDTO>()
            .ForMember(
                dest => dest.TotalDuration,
                opt => opt.MapFrom(src => (TimeSpanModel)src.TotalDuration!)
            )
            .ForMember(
                dest => dest.AverageWorkoutDuration,
                opt => opt.MapFrom(src => (TimeSpanModel)src.AverageWorkoutDuration!)
            )
            .ReverseMap();

        CreateMap<TotalUserProgressDTO, TotalUserProgress>()
           .ForMember(
               dest => dest.TotalDuration,
               opt => opt.MapFrom(src => (TimeSpan)src.TotalDuration!)
           )
           .ForMember(
               dest => dest.AverageWorkoutDuration,
               opt => opt.MapFrom(src => (TimeSpan)src.AverageWorkoutDuration!)
           );
    }
}