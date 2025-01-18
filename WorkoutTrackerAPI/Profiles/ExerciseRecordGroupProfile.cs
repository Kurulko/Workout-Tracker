using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs;
using WorkoutTrackerAPI.Data.Models;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Profiles;

public class ExerciseRecordGroupProfile : Profile
{
    public ExerciseRecordGroupProfile()
    {
        CreateMap<ExerciseRecordGroup, ExerciseRecordGroupDTO>()
            .ForMember(
                dest => dest.ExerciseName,
                opt => opt.MapFrom(src => src.Exercise == null ? null : src.Exercise.Name)
            )
            .ForMember(
                dest => dest.Weight,
                opt => opt.MapFrom(src => src.ExerciseRecords.GetTotalWeightValue())
            )
            .ForMember(
                dest => dest.Sets,
                opt => opt.MapFrom(src => src.ExerciseRecords.Count())
            )
            .ForMember(
                dest => dest.ExerciseType,
                opt => opt.MapFrom(src => src.Exercise!.Type)
            )
            .ReverseMap();

        CreateMap<ExerciseRecordGroupDTO, ExerciseRecordGroup>()
            .ForMember(
                dest => dest.Exercise,
                opt => opt.MapFrom(src => default(Exercise?))
            );

        CreateMap<ExerciseRecordGroupCreationDTO, ExerciseRecordGroup>().ReverseMap();
    }
}
