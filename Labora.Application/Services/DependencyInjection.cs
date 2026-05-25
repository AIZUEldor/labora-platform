using Labora.Application.Interfaces;
using Labora.Application.Mappings;
using Labora.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Labora.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IJobService, JobService>();
        services.AddScoped<IJobApplicationService, JobApplicationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<IReviewService, ReviewService>();
        return services;
    }
}