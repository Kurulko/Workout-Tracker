using AutoMapper;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.DTOs;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSets;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;

namespace WorkoutTracker.Application.Profiles.Exercises;

public class ExerciseSetProfile : Profile
{
    public ExerciseSetProfile()
    {
        CreateMap<ExerciseSet, ExerciseSetDTO>()
            .ForMember(
                dest => dest.ExerciseName,
                opt => opt.MapFrom(src => src.Exercise!.Name)
            )
            .ForMember(
                dest => dest.TotalWeight,
                opt => opt.MapFrom(src => src.GetTotalWeightValue())
            )
            .ForMember(
                dest => dest.TotalTime,
                opt => opt.MapFrom(src => src.GetTotalTimeValue()!)
            )
            .ForMember(
                dest => dest.TotalReps,
                opt => opt.MapFrom(src => src.Reps)
            )
            .ForMember(
                dest => dest.Time,
                opt => opt.MapFrom(src => src.Time.HasValue ? (TimeSpanModel?)src.Time.Value : null)
            )
            .ForMember(
                dest => dest.ExerciseType,
                opt => opt.MapFrom(src => src.Exercise!.Type)
            )
            .ForMember(
                dest => dest.ExercisePhoto,
                opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Image : null)
            )
            .ReverseMap();

        CreateMap<ExerciseSetDTO, ExerciseSet>()
            .ForMember(
                dest => dest.Time,
                opt => opt.MapFrom(src => src.Time.HasValue ? (TimeSpan?)src.Time.Value : null)
            );


        CreateMap<ExerciseSetCreationDTO, ExerciseSet>()
            .ForMember(
                dest => dest.Time,
                opt => opt.MapFrom(src => src.Time.HasValue ? (TimeSpan?)src.Time.Value : null)
            )
            .ReverseMap();
    }
}