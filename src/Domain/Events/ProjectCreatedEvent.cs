using Domain.Common;
using Domain.Projects;

namespace Domain.Events;

/// <summary>
/// Доменное событие, возникающее при создании нового проекта.
/// </summary>
public class ProjectCreatedEvent : BaseEvent
{
    /// <summary>
    /// Возвращает созданный проект, с которым связано данное событие.
    /// </summary>
    /// <value>
    /// Экземпляр <see cref="Project"/>, переданный при создании события.
    /// </value>
    public Project Project { get; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProjectCreatedEvent"/>.
    /// </summary>
    /// <param name="project">Созданный проект.</param>
    /// <exception cref="ArgumentNullException">
    /// Выбрасывается, если <paramref name="project"/> равен <see langword="null"/>.
    /// </exception>
    public ProjectCreatedEvent(Project project)
    {
        Project = project ?? throw new ArgumentNullException(nameof(project));
    }
}