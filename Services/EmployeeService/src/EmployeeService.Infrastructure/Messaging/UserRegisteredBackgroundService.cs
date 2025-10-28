using AuthService.Contracts.IntegrationEvents;
using EmployeeService.Domain.Entities;
using EmployeeService.Domain.Types;
using EmployeeService.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace EmployeeService.Infrastructure.Messaging;

public class UserRegisteredBackgroundService : BackgroundService
{
    private const string QueueName = "user.registered";

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<UserRegisteredBackgroundService> _logger;
    private readonly IConfiguration _configuration;

    private IConnection? _connection;
    private IModel? _channel;

    public UserRegisteredBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<UserRegisteredBackgroundService> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _configuration = configuration;
    }

    private void EnsureConnection()
    {
        if (_connection != null && _channel != null)
            return;

        var hostName =
            _configuration["RabbitMQ:Host"] ??
            Environment.GetEnvironmentVariable("RabbitMQ__Host") ??
            "rabbitmq";

        var factory = new ConnectionFactory
        {
            HostName = hostName
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        EnsureConnection();

        var consumer = new EventingBasicConsumer(_channel);

        consumer.Received += async (_, ea) =>
        {
            try
            {
                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var message = JsonSerializer.Deserialize<UserRegisteredEvent>(json);

                if (message is null)
                {
                    _logger.LogWarning("Invalid UserRegisteredEvent payload: {Payload}", json);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<EmployeeDbContext>();

                var employee = new Employee
                {
                    Id = Guid.NewGuid(),
                    UserId = message.UserId,
                    FirstName = message.Email,   // placeholder
                    LastName = string.Empty,
                    Email = message.Email,
                    HireDate = DateTime.UtcNow,
                    BirthDate = DateTime.UtcNow,
                    Role = EmployeeRole.Staff,
                    Salary = 0m
                };

                db.Employees.Add(employee);
                await db.SaveChangesAsync(stoppingToken);

                _logger.LogInformation(
                    "Created Employee {EmployeeId} for UserId {UserId}",
                    employee.Id, message.UserId
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing UserRegisteredEvent");
            }
        };

        _channel.BasicConsume(
            queue: QueueName,
            autoAck: true,
            consumer: consumer
        );

        _logger.LogInformation("EmployeeService is consuming RabbitMQ queue '{QueueName}'", QueueName);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
