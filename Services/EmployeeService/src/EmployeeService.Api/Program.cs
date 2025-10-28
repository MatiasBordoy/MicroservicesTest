using AuthService.API.Middlewares;
using EmployeeService.API.Middlewares;
using EmployeeService.Application.DependencyInjection;
using EmployeeService.Infrastructure.DependencyInjection;
using EmployeeService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using System.Text;

try
{
    #region Startup

    Log.Information("🚀 Starting EmployeeService API Initialization...");
    var builder = WebApplication.CreateBuilder(args);
    Log.Information("🧱 WebApplication Builder created.");

    #endregion Startup

    #region Serilog Configuration

    Log.Information("⚙️  Configuring Serilog...");

    Log.Logger = new LoggerConfiguration()
        .ReadFrom.Configuration(builder.Configuration)
        .Enrich.FromLogContext()
        .CreateLogger();

    builder.Host.UseSerilog();

    Log.Information("✅ Serilog configured successfully.");

    #endregion Serilog Configuration

    #region Application Layer

    Log.Information("🔌 Injecting Application Layer...");
    builder.Services.AddApplication();
    Log.Information("✅ Application Layer injected.");

    #endregion Application Layer

    #region Infrastructure Layer

    Log.Information("🗄️  Injecting Infrastructure Layer...");

    string envDbConn = Environment.GetEnvironmentVariable("ConnectionStrings__Default") ?? string.Empty;
    string appSettingsDbConn = builder.Configuration.GetConnectionString("Default") ?? string.Empty;
    string dbConnectionString = !string.IsNullOrWhiteSpace(envDbConn) ? envDbConn : appSettingsDbConn;

    if (string.IsNullOrWhiteSpace(dbConnectionString))
        throw new InvalidOperationException("❌ No valid connection string found. Check environment or appsettings.json.");

    Log.Information("✅ Infrastructure Layer: using connection string '{DbConnectionString}'", dbConnectionString);
    builder.Services.AddInfrastructure(dbConnectionString);

    #endregion Infrastructure Layer

    #region Redis Cache

    Log.Information("🔁 Configuring Redis Cache...");

    string? envRedisHost = Environment.GetEnvironmentVariable("Redis__Host");
    string? appSettingsRedisHost = builder.Configuration.GetConnectionString("Cache");
    string? redisHost = envRedisHost ?? appSettingsRedisHost;

    if (string.IsNullOrWhiteSpace(redisHost))
        throw new InvalidOperationException("❌ Redis host not found. Check environment or appsettings.json.");

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisHost;
        options.InstanceName = "MicroservicesExample:";
    });

    Log.Information("✅ Redis configured successfully. Host: {RedisHost}", redisHost);

    #endregion Redis Cache

    #region Auth

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

    builder.Services.AddAuthorization();

    #endregion Auth

    #region Controllers & Swagger

    Log.Information("🧭 Registering Controllers and Swagger...");
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    Log.Information("✅ Controllers and Swagger configured.");

    #endregion Controllers & Swagger

    #region Build Application

    var app = builder.Build();
    Log.Information("🏗️  Application built successfully.");

    #endregion Build Application

    #region Database Migrations

    Log.Information("📦 Executing SQL Server migrations...");

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();
        db.Database.Migrate();
    }

    Log.Information("✅ Database migrations applied successfully.");

    #endregion Database Migrations

    #region HTTP Pipeline

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        Log.Information("🧪 Swagger enabled (Development).");
    }

    app.UseHttpsRedirection();
    app.MapControllers();

    // Correlation ID → before everything
    app.UseCorrelationId();

    // Request/Response Body Logging
    app.UseRequestResponseLogging();

    // Automatic Log requests HTTP
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate =
            "🌐 HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.000} ms (TraceId: {TraceId})";

        options.GetLevel = (ctx, _, ex) =>
            ex != null || ctx.Response.StatusCode >= 500 ? LogEventLevel.Error :
            ctx.Response.StatusCode >= 400 ? LogEventLevel.Warning :
            LogEventLevel.Information;
    });

    Log.Information("🟢 EmployeeService API ready to run!");

    #endregion HTTP Pipeline

    #region Run

    app.Run();

    #endregion Run
}
catch (Exception ex)
{
    Log.Fatal(ex, "💀 EmployeeService API failed to start.");
}
finally
{
    Log.CloseAndFlush();
}


public partial class Program { }