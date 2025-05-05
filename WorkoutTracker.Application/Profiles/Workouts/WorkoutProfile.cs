using AutoMapper;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.DTOs.Workouts.Workouts;
using WorkoutTracker.Domain.Entities.Workouts;

namespace WorkoutTracker.Application.Profiles.Workouts;

public class WorkoutProfile : Profile
{
    public WorkoutProfile()
    {
        CreateMap<Workout, WorkoutDTO>()
            .ForMember(
                dest => dest.Exercises,
                opt => opt.MapFrom(src => src.ExerciseSetGroups!.GetExercises())
            )
            .ForMember(
                dest => dest.Weight,
                opt => opt.MapFrom(src => src.ExerciseSetGroups!.GetTotalWeightValue())
            )
            
            .ForMember(
                dest => dest.Started,
                opt => opt.MapFrom(src => src.WorkoutRecords!.MinBy(wr => wr.Date)!.Date)
            )
            .ReverseMap();

        CreateMap<WorkoutCreationDTO, Workout>().ReverseMap();
    }
}
