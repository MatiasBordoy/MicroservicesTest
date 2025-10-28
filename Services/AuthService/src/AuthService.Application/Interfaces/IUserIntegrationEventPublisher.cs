using AuthService.Contracts.IntegrationEvents;

namespace AuthService.Application.Interfaces;

public interface IUserIntegrationEventPublisher
{
    Task PublishUserRegisteredAsync(UserRegisteredEvent @event);
}
