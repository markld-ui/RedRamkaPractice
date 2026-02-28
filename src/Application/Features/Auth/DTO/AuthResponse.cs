namespace Application.Features.Auth.DTO;

/// <summary>
/// Ответ на успешную аутентификацию или обновление токена.
/// </summary>
public class AuthResponse
{
    /// <summary>JWT-токен доступа для выполнения авторизованных запросов.</summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Токен для обновления токена доступа.
    /// Равен <see langword="null"/>, если не был выдан.
    /// </summary>
    public string? RefreshToken { get; set; }
}