namespace Domain.Models;

/// <summary>
/// Связующая сущность между пользователем и ролью.
/// </summary>
public class UserRole
{
    /// <summary>Внешний ключ, ссылающийся на пользователя.</summary>
    public Guid UserId { get; set; }

    /// <summary>Внешний ключ, ссылающийся на роль.</summary>
    public Guid RoleId { get; set; }

    /// <summary>Навигационное свойство пользователя.</summary>
    public User User { get; set; } = null!;

    /// <summary>Навигационное свойство роли.</summary>
    public Role Role { get; set; } = null!;
}