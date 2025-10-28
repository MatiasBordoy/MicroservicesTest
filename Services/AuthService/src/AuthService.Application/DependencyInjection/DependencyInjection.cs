using AuthService.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, Services.AuthService>();

        return services;
    }
}
