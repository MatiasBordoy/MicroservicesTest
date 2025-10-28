using AuthService.Application.Common;
using AuthService.Application.Interfaces;
using AuthService.Contracts.DTOs;
using AuthService.Contracts.IntegrationEvents;
using AuthService.Contracts.Persistence;
using AuthService.Domain.Entities;

namespace AuthService.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;
    private readonly IUserIntegrationEventPublisher _eventPublisher;

    public AuthService(
        IUserRepository userRepository,
        IJwtService jwtService,
        IUserIntegrationEventPublisher eventPublisher)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
        _eventPublisher = eventPublisher;
    }

    public async Task<Guid> CreateUserAsync(CreateUserRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new Exception("User already exists");

        var hash = PasswordHasher.Hash(request.Password);

        var user = new User(request.Email, hash, request.Role);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Publish integration event
        var integrationEvent = new UserRegisteredEvent
        {
            UserId = user.Id,
            Email = user.Email,
            Role = user.Role
        };

        await _eventPublisher.PublishUserRegisteredAsync(integrationEvent);

        return user.Id;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null)
            throw new Exception("Invalid credentials");

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
            throw new Exception("Invalid credentials");

        var token = _jwtService.GenerateToken(user);

        return new LoginResponse
        (
            Token: token,
            Role: user.Role,
            UserId: user.Id
        );
    }
}
