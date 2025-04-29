using AutoMapper;
using WorkoutTrackerAPI.Data;
using WorkoutTrackerAPI.Data.DTOs.ProgressDTOs;
using WorkoutTrackerAPI.Data.Models.ProgressModels;

namespace WorkoutTrackerAPI.Profiles;

public class WorkoutProgressProfile : Profile
{
    public WorkoutProgressProfile()
    {
        CreateMap<CurrentUserProgress, CurrentUserProgressDTO>()
            .ReverseMap();

        CreateMap<WorkoutProgress, WorkoutProgressDTO>()
            .ReverseMap();

        CreateMap<BaseInfoProgress, BaseInfoProgressDTO>()
            .ReverseMap();

        CreateMap<TotalCompletedProgress, TotalCompletedProgressDTO>()
            .ReverseMap();

        CreateMap<WorkoutWeightLiftedProgress, WorkoutWeightLiftedProgressDTO>()
            .ReverseMap();

        CreateMap<StrikeDurationProgress, StrikeDurationProgressDTO>()
            .ReverseMap();

        CreateMap<WorkoutDurationProgress, WorkoutDurationProgressDTO>()
            .ReverseMap();

        CreateMap<BodyWeightProgress, BodyWeightProgressDTO>()
            .ReverseMap();
    }
}