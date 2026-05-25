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

        // JobApplication mappings
        CreateMap<JobApplication, ApplicationResponseDto>();
        CreateMap<ApplicationRequestDto, JobApplication>();

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