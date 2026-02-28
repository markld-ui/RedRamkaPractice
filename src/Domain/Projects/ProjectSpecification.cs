namespace Domain.Projects;

/// <summary>
/// Спецификация проекта, описывающая его требования в рамках конкретной версии.
/// </summary>
/// <remarks>
/// В каждый момент времени актуальной может быть только одна утверждённая спецификация.
/// Управление утверждением осуществляется через методы <see cref="Approve"/> и <see cref="Revoke"/>.
/// </remarks>
public class ProjectSpecification
{
    /// <summary>Уникальный идентификатор спецификации.</summary>
    public Guid Id { get; private set; }

    /// <summary>Порядковый номер версии спецификации в рамках проекта.</summary>
    public int Version { get; private set; }

    /// <summary>Содержимое спецификации.</summary>
    public string Content { get; private set; } = null!;

    /// <summary>
    /// Признак утверждённости спецификации.
    /// <see langword="true"/> если спецификация утверждена; иначе <see langword="false"/>.
    /// </summary>
    public bool IsApproved { get; private set; }

    /// <summary>Дата и время создания спецификации в формате UTC.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>Дата и время утверждения спецификации в формате UTC.</summary>
    public DateTime ApprovedAt { get; private set; }

    /// <summary>Внешний ключ, ссылающийся на проект.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Навигационное свойство проекта.</summary>
    public Project Project { get; private set; } = null!;

    private ProjectSpecification() { } // EF

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProjectSpecification"/>.
    /// </summary>
    /// <param name="version">Номер версии спецификации.</param>
    /// <param name="content">Содержимое спецификации.</param>
    /// <param name="projectId">Идентификатор проекта, которому принадлежит спецификация.</param>
    public ProjectSpecification(int version, string content, Guid projectId)
    {
        Id = Guid.NewGuid();
        Version = version;
        Content = content;
        ProjectId = projectId;
        CreatedAt = DateTime.UtcNow;
        IsApproved = false;
    }

    /// <summary>
    /// Утверждает спецификацию и фиксирует время утверждения.
    /// Если спецификация уже утверждена, вызов игнорируется.
    /// </summary>
    public void Approve()
    {
        if (IsApproved) return;

        IsApproved = true;
        ApprovedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Отзывает утверждение спецификации.
    /// Если спецификация не утверждена, вызов игнорируется.
    /// </summary>
    public void Revoke()
    {
        if (!IsApproved) return;
        IsApproved = false;
    }
}