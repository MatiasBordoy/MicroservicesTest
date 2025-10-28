using EmployeeService.Contracts.Persistence;
using EmployeeService.Infrastructure.Persistence;
using EmployeeService.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeService.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<EmployeeDbContext>(options =>
                options.UseSqlServer(connectionString));

            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            return services;
        }
    }
}
