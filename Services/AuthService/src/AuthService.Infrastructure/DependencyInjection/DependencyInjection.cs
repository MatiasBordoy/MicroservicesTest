using AuthService.Application.Interfaces;
using AuthService.Contracts.Persistence;
using AuthService.Infrastructure.Authentication;
using AuthService.Infrastructure.Messaging;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<AuthDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IJwtService, JwtService>();

            services.AddSingleton<IUserIntegrationEventPublisher, RabbitMqUserIntegrationEventPublisher>();

            return services;
        }
    }
}
