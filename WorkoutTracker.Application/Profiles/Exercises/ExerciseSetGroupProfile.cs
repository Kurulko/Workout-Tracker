using AutoMapper;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseSets;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseSets.ExerciseSetGroups;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Domain.Entities.Workouts;
using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.Application.Profiles.Exercises;

public class ExerciseSetGroupProfile : Profile
{
    public ExerciseSetGroupProfile()
    {
        CreateMap<ExerciseSetGroup, ExerciseSetGroupDTO>()
            .ForMember(
                dest => dest.Weight,
                opt => opt.MapFrom(src => src.ExerciseSets.GetTotalWeightValue())
            )
            .ForMember(
                dest => dest.Sets,
                opt => opt.MapFrom(src => src.ExerciseSets.Count())
            )
            .ForMember(
                dest => dest.ExerciseName,
                opt => opt.MapFrom(src => src.Exercise == null ? null : src.Exercise.Name)
            )
            .ForMember(
                dest => dest.WorkoutName,
                opt => opt.MapFrom(src => src.Workout == null ? null : src.Workout.Name)
            )
            .ForMember(
                dest => dest.ExerciseType,
                opt => opt.MapFrom(src => src.Exercise!.Type)
            )
            .ReverseMap();

        CreateMap<ExerciseSetGroupDTO, ExerciseSetGroup>()
            .ForMember(
                dest => dest.Exercise,
                opt => opt.MapFrom(src => default(Exercise?))
            )
            .ForMember(
                dest => dest.Workout,
                opt => opt.MapFrom(src => default(Workout?))
            );

        CreateMap<ExerciseSetGroupCreationDTO, ExerciseSetGroup>().ReverseMap();
    }
}
