using AutoMapper;
using WorkoutTracker.Application.DTOs.Progresses;
using WorkoutTracker.Application.Models.Progresses;

namespace WorkoutTracker.Application.Profiles.Workouts;

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