using AuthService.Application.Interfaces;
using AuthService.Contracts.IntegrationEvents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace AuthService.Infrastructure.Messaging;

public class RabbitMqUserIntegrationEventPublisher : IUserIntegrationEventPublisher, IDisposable
{
    private const string QueueName = "user.registered";

    private readonly ILogger<RabbitMqUserIntegrationEventPublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqUserIntegrationEventPublisher(
        IConfiguration configuration,
        ILogger<RabbitMqUserIntegrationEventPublisher> logger)
    {
        _logger = logger;

        var hostName =
            configuration["RabbitMQ:Host"] ??
            Environment.GetEnvironmentVariable("RabbitMQ__Host") ??
            "rabbitmq";

        var factory = new ConnectionFactory
        {
            HostName = hostName
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        // Declara la cola (idempotente)
        _channel.QueueDeclare(
            queue: QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null
        );
    }

    public Task PublishUserRegisteredAsync(UserRegisteredEvent @event)
    {
        var payload = JsonSerializer.Serialize(@event);
        var body = Encoding.UTF8.GetBytes(payload);

        _channel.BasicPublish(
            exchange: "",
            routingKey: QueueName,
            basicProperties: null,
            body: body
        );

        _logger.LogInformation(
            "Published UserRegisteredEvent for UserId {UserId} to queue {QueueName}",
            @event.UserId, QueueName
        );

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
    }
}
