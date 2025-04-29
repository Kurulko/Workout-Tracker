using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.DTOs.UserDTOs.ExerciseRecordDTOs;
using WorkoutTrackerAPI.Data.Models.UserModels;
using WorkoutTrackerAPI.Extentions;

namespace WorkoutTrackerAPI.Profiles;

public class ExerciseRecordProfile : Profile
{
    public ExerciseRecordProfile()
    {
        CreateMap<ExerciseRecord, ExerciseRecordDTO>()
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
                opt => opt.MapFrom(src => src.Exercise!.Type )
            )
            .ForMember(
                dest => dest.ExercisePhoto,
                opt => opt.MapFrom(src => src.Exercise != null ? src.Exercise.Image : null)
            )
            .ReverseMap();

        CreateMap<ExerciseRecordDTO, ExerciseRecord>()
            .ForMember(
                dest => dest.Time,
                opt => opt.MapFrom(src => src.Time.HasValue ? (TimeSpan?)src.Time.Value : null)
            );

        CreateMap<ExerciseRecord, ExerciseRecordCreationDTO>()
            .ForMember(
                dest => dest.Time,
                opt => opt.MapFrom(src => src.Time.HasValue ? (TimeSpanModel?)src.Time.Value : null)
            )
            .ReverseMap();

        CreateMap<ExerciseRecordCreationDTO, ExerciseRecord>()
           .ForMember(
               dest => dest.Time,
               opt => opt.MapFrom(src => src.Time.HasValue ? (TimeSpan?)src.Time.Value : null)
           );
        
        CreateMap<ExerciseRecordUpdateDTO, ExerciseRecord>()
           .ForMember(
               dest => dest.Time,
               opt => opt.MapFrom(src => src.Time.HasValue ? (TimeSpan?)src.Time.Value : null)
           ).ReverseMap(); 
    }
}