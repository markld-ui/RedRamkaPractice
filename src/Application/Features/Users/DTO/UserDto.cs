namespace Application.Features.Users.DTO;

/// <summary>
/// DTO с данными пользователя, возвращаемыми в ответ на запросы.
/// </summary>
public class UserDto
{
    /// <summary>Уникальный идентификатор пользователя.</summary>
    public Guid Id { get; set; }

    /// <summary>Имя пользователя.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Фамилия пользователя.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Адрес электронной почты пользователя.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Список названий ролей, назначенных пользователю.</summary>
    public List<string> Roles { get; set; } = new();
}