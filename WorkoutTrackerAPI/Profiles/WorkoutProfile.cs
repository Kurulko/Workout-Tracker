using AutoMapper;
using WorkoutTrackerAPI.Data.DTOs;
using WorkoutTrackerAPI.Data.Models;

namespace WorkoutTrackerAPI.Profiles;

public class WorkoutProfile : Profile
{
    public WorkoutProfile()
    {
        CreateMap<WorkoutDTO, Workout>().ReverseMap();
    }
}
