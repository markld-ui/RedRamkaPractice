namespace Application.Features.Auth.DTO;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string?  RefreshToken { get; set; }
}