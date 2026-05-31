using AutoMapper;
using Labora.Application.DTOs.Applications;
using Labora.Application.DTOs.Categories;
using Labora.Application.DTOs.Jobs;
using Labora.Application.DTOs.Reviews;
using Labora.Application.DTOs.Transactions;
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
        CreateMap<ApplicationRequestDto, JobApplication>();
        CreateMap<JobApplication, ApplicationResponseDto>()
     .ForMember(dest => dest.WorkerName,
         opt => opt.MapFrom(src => src.Worker != null
             ? $"{src.Worker.FirstName} {src.Worker.LastName}"
             : null))
     .ForMember(dest => dest.WorkerCvUrl,
         opt => opt.MapFrom(src => src.Worker != null
             ? src.Worker.CvUrl
             : null));

        // User mappings
        CreateMap<User, UserProfileResponseDto>();
        CreateMap<UpdateProfileRequestDto, User>();

        // Category mappings
        CreateMap<Category, CategoryResponseDto>()
            .ForMember(dest => dest.SubCategories,
                opt => opt.MapFrom(src => src.SubCategories));
        CreateMap<CategoryRequestDto, Category>();

        // Transaction mappings
        CreateMap<Transaction, TransactionResponseDto>();
        CreateMap<TransactionRequestDto, Transaction>();

        // Review mappings
        CreateMap<Review, ReviewResponseDto>()
            .ForMember(dest => dest.ReviewerFirstName,
                opt => opt.MapFrom(src => src.Reviewer.FirstName))
            .ForMember(dest => dest.ReviewerLastName,
                opt => opt.MapFrom(src => src.Reviewer.LastName));
        CreateMap<ReviewRequestDto, Review>();
    }
}