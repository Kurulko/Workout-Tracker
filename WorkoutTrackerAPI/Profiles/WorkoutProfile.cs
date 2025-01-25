using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Profiles;

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

        //CreateMap<Workout, WorkoutDetailsDTO>()
        //    .ForMember(
        //        dest => dest.Workout,
        //        opt => opt.MapFrom(src => src)
        //    )
        //    .ForMember(
        //        dest => dest.TotalWorkouts,
        //        opt => opt.MapFrom(src => src.WorkoutRecords!.Count())
        //    )
        //    .ForMember(
        //        dest => dest.TotalWeight,
        //        opt => opt.MapFrom(src => src.WorkoutRecords!.GetTotalWeightValue())
        //    )
        //    .ForMember(
        //        dest => dest.TotalDuration,
        //        opt => opt.MapFrom(src => (TimeSpanModel)src.WorkoutRecords!.GetTotalTimeValue()!)
        //    )
        //    .ForMember(
        //        dest => dest.Muscles,
        //        opt => opt.MapFrom(src => src.ExerciseSetGroups!.GetMuscles())
        //    )
        //    .ForMember(
        //        dest => dest.Equipments,
        //        opt => opt.MapFrom(src => src.ExerciseSetGroups!.GetEquipments())
        //    )
        //  .ReverseMap();

        CreateMap<WorkoutCreationDTO, Workout>().ReverseMap();
    }
}
