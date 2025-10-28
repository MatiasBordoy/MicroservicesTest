using Serilog;

try
{
    #region Startup

    Log.Information("🚀 Starting AuthService.API Initialization...");
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

    #region HTTP Pipeline

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        Log.Information("🧪 Swagger enabled (Development).");
    }

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("🟢 AuthService.API ready to run!");

    #endregion HTTP Pipeline

    #region Run

    app.Run();

    #endregion Run
}
catch (Exception ex)
{
    Log.Fatal(ex, "💀 AuthService.API failed to start.");
}
finally
{
    Log.CloseAndFlush();
}
