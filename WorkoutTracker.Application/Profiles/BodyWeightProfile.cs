using AutoMapper;
using WorkoutTracker.Application.DTOs;
using WorkoutTracker.Domain.Entities;

namespace WorkoutTracker.Application.Profiles;

public class BodyWeightProfile : Profile
{
    public BodyWeightProfile()
    {
        CreateMap<BodyWeightDTO, BodyWeight>().ReverseMap();
    }
}