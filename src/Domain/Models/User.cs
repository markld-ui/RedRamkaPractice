using Domain.Projects;

namespace Domain.Models;

/// <summary>
/// Пользователь системы.
/// </summary>
public class User
{
    /// <summary>Уникальный идентификатор пользователя.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Имя пользователя.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Фамилия пользователя.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Учётные данные пользователя, используемые для аутентификации.</summary>
    public Credentials Credentials { get; set; } = null!;

    /// <summary>Коллекция проектов, участником которых является пользователь.</summary>
    public ICollection<ProjectMember> ProjectMemberships { get; set; } =
        new List<ProjectMember>();

    /// <summary>Коллекция ролей, назначенных пользователю.</summary>
    public ICollection<UserRole> UserRoles { get; set; } =
        new List<UserRole>();
}