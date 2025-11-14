namespace AuthService.Contracts.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = default!;
        public string Role { get; set; } = default!;
        public Guid UserId { get; set; }
    }
}