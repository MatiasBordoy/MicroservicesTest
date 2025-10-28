using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace EmployeeService.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EmployeeDbContext>
    {
        public EmployeeDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../EmployeeService.API");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = configuration.GetConnectionString("Default")
                ?? "data source=localhost; initial catalog=MicroservicesExample.EmployeeService; Integrated Security=True; MultipleActiveResultSets=True;TrustServerCertificate=true;";

            var optionsBuilder = new DbContextOptionsBuilder<EmployeeDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new EmployeeDbContext(optionsBuilder.Options);
        }
    }
}
