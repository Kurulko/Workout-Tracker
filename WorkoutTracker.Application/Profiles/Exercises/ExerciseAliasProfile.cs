using AutoMapper;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseAliases;
using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.Application.Profiles.Exercises;

public class ExerciseAliasProfile : Profile
{
    public ExerciseAliasProfile()
    {
        CreateMap<ExerciseAliasDTO, ExerciseAlias>()
            .ReverseMap();
    }
}