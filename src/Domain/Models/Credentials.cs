namespace Domain.Models;

/// <summary>
/// Учётные данные пользователя, используемые для аутентификации.
/// </summary>
public class Credentials
{
    /// <summary>Уникальный идентификатор записи учётных данных.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Адрес электронной почты пользователя.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Хэш пароля пользователя.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// Refresh-токен для обновления токена доступа.
    /// Равен <see langword="null"/>, если токен не был выдан или был отозван.
    /// </summary>
    public string? RefreshToken { get; set; }

    /// <summary>Внешний ключ, ссылающийся на владельца учётных данных.</summary>
    public Guid UserId { get; set; }

    /// <summary>Навигационное свойство пользователя — владельца учётных данных.</summary>
    public User User { get; set; } = null!;
}