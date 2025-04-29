using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.WorkoutModels;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Profiles;

public class WorkoutRecordProfile : Profile
{
    public WorkoutRecordProfile()
    {
        CreateMap<WorkoutRecord, WorkoutRecordDTO>()
            .ForMember(
                dest => dest.WorkoutName,
                opt => opt.MapFrom(src => src.Workout!.Name)
            )
            .ForMember(
                dest => dest.Time,
                opt => opt.MapFrom(src => src.Time!)
            )
            .ForMember(
                dest => dest.Exercises,
                opt => opt.MapFrom(src => src.ExerciseRecordGroups.GetExercises())
            )
            .ForMember(
                dest => dest.Weight,
                opt => opt.MapFrom(src => src.ExerciseRecordGroups.GetTotalWeightValue())
            )
            .ReverseMap();

        CreateMap<WorkoutRecordDTO, WorkoutRecord>()
           .ForMember(
               dest => dest.Time,
               opt => opt.MapFrom(src => src.Time!)
           );

        CreateMap<WorkoutRecordCreationDTO, WorkoutRecord>()
            .ForMember(
                dest => dest.Time,
                opt => opt.MapFrom(src => src.Time!)
            )
            .ReverseMap();
    }
}
