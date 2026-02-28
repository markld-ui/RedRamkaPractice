namespace Domain.Models;

/// <summary>
/// Роль пользователя в системе.
/// </summary>
/// <remarks>
/// Предусмотрены следующие роли: <c>Developer</c>, <c>Tester</c>,
/// <c>ProductManager</c>, <c>ProjectManager</c>, <c>DevOps</c>, <c>Admin</c>.
/// </remarks>
public class Role
{
    /// <summary>Уникальный идентификатор роли.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Название роли.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Описание роли и её зоны ответственности.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Коллекция пользователей, обладающих данной ролью.</summary>
    public ICollection<User> Users { get; set; } = new List<User>();

    /// <summary>Коллекция связей пользователей с данной ролью.</summary>
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}