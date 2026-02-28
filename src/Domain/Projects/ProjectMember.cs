namespace Domain.Projects;

/// <summary>
/// Участник проекта, связывающий пользователя с проектом и его ролью в нём.
/// </summary>
public class ProjectMember
{
    private ProjectMember() { } // EF

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProjectMember"/>.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта.</param>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="roleId">Идентификатор роли пользователя в проекте.</param>
    public ProjectMember(
        Guid projectId,
        Guid userId,
        Guid roleId)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        UserId = userId;
        RoleId = roleId;
        JoinedAt = DateTime.UtcNow;
    }

    /// <summary>Уникальный идентификатор записи об участнике.</summary>
    public Guid Id { get; private set; }

    /// <summary>Идентификатор пользователя — участника проекта.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Идентификатор роли пользователя в проекте.</summary>
    public Guid RoleId { get; private set; }

    /// <summary>Дата и время вступления пользователя в проект в формате UTC.</summary>
    public DateTime JoinedAt { get; private set; }

    /// <summary>Внешний ключ, ссылающийся на проект.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Навигационное свойство проекта.</summary>
    public Project Project { get; private set; } = null!;
}