namespace Domain.Projects;

/// <summary>
/// Запись об одном переходе между стадиями жизненного цикла проекта.
/// </summary>
public class ProjectTransition
{
    private ProjectTransition() { } // EF

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProjectTransition"/>.
    /// </summary>
    /// <param name="projectId">Идентификатор проекта, в котором произошёл переход.</param>
    /// <param name="from">Исходная стадия проекта.</param>
    /// <param name="to">Целевая стадия проекта.</param>
    /// <param name="reason">Причина перехода (опционально).</param>
    public ProjectTransition(
        Guid projectId,
        ProjectStage from,
        ProjectStage to,
        string? reason)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        FromStage = from;
        ToStage = to;
        Reason = reason;
        ChangedAt = DateTime.UtcNow;
    }

    /// <summary>Уникальный идентификатор записи о переходе.</summary>
    public Guid Id { get; private set; }

    /// <summary>Идентификатор проекта, в котором произошёл переход.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Исходная стадия до перехода.</summary>
    public ProjectStage FromStage { get; private set; }

    /// <summary>Целевая стадия после перехода.</summary>
    public ProjectStage ToStage { get; private set; }

    /// <summary>
    /// Причина перехода между стадиями.
    /// Равна <see langword="null"/> для переходов, не требующих обоснования.
    /// </summary>
    public string? Reason { get; private set; }

    /// <summary>Дата и время перехода в формате UTC.</summary>
    public DateTime ChangedAt { get; private set; }

    /// <summary>Навигационное свойство проекта.</summary>
    public Project Project { get; private set; } = null!;
}