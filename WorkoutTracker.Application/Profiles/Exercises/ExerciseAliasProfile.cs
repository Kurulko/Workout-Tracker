using AutoMapper;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseAliases;
using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.Application.Profiles.Exercises;

public class ExerciseAliasProfile : Profile
{
    public ExerciseAliasProfile()
    {
        CreateMap<ExerciseAlias, ExerciseAliasDTO>()
            .ForMember(
                dest => dest.ExerciseName,
                opt => opt.MapFrom(src => src.Exercise!.Name)
            )
            .ReverseMap();
    }
}