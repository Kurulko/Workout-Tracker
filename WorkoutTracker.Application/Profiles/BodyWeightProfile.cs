using AutoMapper;
using WorkoutTracker.Application.DTOs.BodyWeights;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Profiles;

public class BodyWeightProfile : Profile
{
    public BodyWeightProfile()
    {
        CreateMap<BodyWeightDTO, BodyWeight>().ReverseMap();
        CreateMap<BodyWeightCreationDTO, BodyWeight>().ReverseMap();
    }
}