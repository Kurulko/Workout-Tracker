using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.MuscleDTOs;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Data.DTOs.WorkoutDTOs.ExerciseAliasDTOs;

namespace WorkoutTrackerAPI.Profiles;

public class ExerciseAliasProfile : Profile
{
    public ExerciseAliasProfile()
    {
        CreateMap<ExerciseAliasDTO, ExerciseAlias>()
            .ReverseMap();
    }
}