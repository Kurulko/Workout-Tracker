using AutoMapper;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.DTOs.Exercises.ExerciseRecords.ExerciseRecordGroups;
using WorkoutTracker.Domain.Entities.Exercises;
using WorkoutTracker.Domain.Entities.Exercises.ExerciseGroups;

namespace WorkoutTracker.Application.Profiles.Exercises;

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
