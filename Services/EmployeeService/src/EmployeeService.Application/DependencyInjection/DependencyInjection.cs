using Microsoft.Extensions.DependencyInjection;
using EmployeeService.Application.Interfaces;

namespace EmployeeService.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IEmployeeService, Services.EmployeeService>();
            return services;
        }
    }
}
