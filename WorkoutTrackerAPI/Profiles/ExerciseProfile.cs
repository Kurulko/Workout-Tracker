using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Profiles;

public class ExerciseProfile : Profile
{
    public ExerciseProfile()
    {
        CreateMap<ExerciseDTO, Exercise>().ReverseMap();
    }
}