using Labora.Application.Interfaces;
using Labora.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Labora.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        return services;
    }
}