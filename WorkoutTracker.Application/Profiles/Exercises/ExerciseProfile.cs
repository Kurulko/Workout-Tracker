using AutoMapper;
using WorkoutTracker.Application.Common.Extensions;
using WorkoutTracker.Application.DTOs.Exercises.Exercises;
using WorkoutTracker.Application.DTOs.Muscles.Muscles;
using WorkoutTracker.Domain.Entities.Exercises;

namespace WorkoutTracker.Application.Profiles.Exercises;

public class ExerciseProfile : Profile
{
    public ExerciseProfile()
    {
        CreateMap<Exercise, ExerciseDTO>()
            .ForMember(
                dest => dest.IsCreatedByUser,
                opt => opt.MapFrom(src => src.CreatedByUserId != null)
            )
            .ForMember(
                dest => dest.WorkingMuscles,
                opt => opt.MapFrom(src => src.WorkingMuscles.GetModelsOrEmpty().Select(cm => new ChildMuscleDTO() { Id = cm.Id, Name = cm.Name }))
            ) 
            .ReverseMap();

        CreateMap<Exercise, ExerciseDetailsDTO>()
            .ForMember(
                dest => dest.Exercise,
                opt => opt.MapFrom(src => src)
            )
            .ForMember(
                dest => dest.CountOfTimes,
                opt => opt.MapFrom(src => src.ExerciseRecords!.Count())
            )
            //.ForMember(
            //    dest => dest.SumOfWeight,
            //    opt => opt.MapFrom(src => src.GetTotalWeightValue())
            //)
            //.ForMember(
            //    dest => dest.SumOfTime,
            //    opt => opt.MapFrom(src => src.GetTotalTimeValue().HasValue ? (TimeSpanModel?)src.GetTotalTimeValue()! : null)
            //)
            //.ForMember(
            //    dest => dest.SumOfReps,
            //    opt => opt.MapFrom(src => src.GetTotalRepsValue())
            //)
            .ForMember(
                dest => dest.SumOfWeight,
                opt => opt.MapFrom(src => src.ExerciseRecords!.GetTotalWeightValue())
            )
            .ForMember(
                dest => dest.SumOfTime,
                opt => opt.MapFrom(src => src.ExerciseRecords!.GetTotalTimeValue()!)
            )
            .ForMember(
                dest => dest.SumOfReps,
                opt => opt.MapFrom(src => src.ExerciseRecords!.GetTotalRepsValue())
            )
            .ForMember(
                dest => dest.Dates,
                opt => opt.MapFrom(src => src.ExerciseRecords!.Select(wr => wr.Date).Distinct().ToList())
            )
            .ReverseMap();

        CreateMap<ExerciseCreationDTO, Exercise>().ReverseMap();
        CreateMap<ExerciseUpdateDTO, Exercise>().ReverseMap();
    }
}