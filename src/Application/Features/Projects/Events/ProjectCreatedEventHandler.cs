using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Projects.Events;

/// <summary>
/// Обработчик доменного события <see cref="ProjectCreatedEvent"/>.
/// </summary>
/// <remarks>
/// Выполняет логирование факта создания проекта.
/// </remarks>
public class ProjectCreatedEventHandler : INotificationHandler<ProjectCreatedEvent>
{
    private readonly ILogger<ProjectCreatedEventHandler> _logger;

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="ProjectCreatedEventHandler"/>.
    /// </summary>
    /// <param name="logger">Экземпляр логгера.</param>
    public ProjectCreatedEventHandler(ILogger<ProjectCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Обрабатывает событие создания проекта.
    /// </summary>
    /// <param name="notification">Событие, содержащее данные созданного проекта.</param>
    /// <param name="ct">Токен отмены операции.</param>
    public Task Handle(ProjectCreatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation("Project created: {ProjectId} - {ProjectName}",
            notification.Project.Id,
            notification.Project.Name);

        return Task.CompletedTask;
    }
}