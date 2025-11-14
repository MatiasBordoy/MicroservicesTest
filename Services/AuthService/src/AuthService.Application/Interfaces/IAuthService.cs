using AuthService.Contracts.DTOs;

namespace AuthService.Application.Interfaces;

public interface IAuthService
{
    Task<Guid> CreateUserAsync(CreateUserRequest request);
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
