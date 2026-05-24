using AutoMapper;
using Labora.Application.DTOs.Applications;
using Labora.Application.DTOs.Jobs;
using Labora.Application.DTOs.Users;
using Labora.Domain.Entities;

namespace Labora.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Job mappings
        CreateMap<Job, JobResponseDto>();
        CreateMap<JobRequestDto, Job>();

        // JobApplication mappings
        CreateMap<JobApplication, ApplicationResponseDto>();
        CreateMap<ApplicationRequestDto, JobApplication>();

        // User mappings
        CreateMap<User, UserProfileResponseDto>();
        CreateMap<UpdateProfileRequestDto, User>();
    }
}