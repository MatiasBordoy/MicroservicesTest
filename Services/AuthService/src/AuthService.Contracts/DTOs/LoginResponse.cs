namespace AuthService.Contracts.DTOs
{
    public class LoginResponse
    {
        public string Token { get; set; } = default!;
        public string Role { get; set; } = default!;
        public Guid UserId { get; set; }

        public LoginResponse() { }
        public LoginResponse(string Token, string Role, Guid UserId)
        {
            this.Role = Role;
            this.Token = Token;
            this.UserId = UserId;
        }
    }
}