using Labora.Application.Interfaces;
using Labora.Domain.Interfaces;
using Labora.Infrastructure.Data;
using Labora.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Labora.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<LaboaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IPaymentOrderRepository, PaymentOrderRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IReviewRepository, ReviewRepository>();
        services.AddScoped<IOtpRepository, OtpRepository>();
        services.AddScoped<IOtpAbuseEventRepository, OtpAbuseEventRepository>();
        services.AddScoped<IOtpBlockRepository, OtpBlockRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}