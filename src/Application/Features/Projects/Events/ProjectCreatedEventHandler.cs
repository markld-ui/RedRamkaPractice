using Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Projects.Events;

public class ProjectCreatedEventHandler : INotificationHandler<ProjectCreatedEvent>
{
    private readonly ILogger<ProjectCreatedEventHandler> _logger;

    public ProjectCreatedEventHandler(ILogger<ProjectCreatedEventHandler> logger)
    {
        _logger = logger;
    }

    public Task Handle(ProjectCreatedEvent notification, CancellationToken ct)
    {
        _logger.LogInformation("Project created: {ProjectId} - {ProjectName}",
            notification.Project.Id,
            notification.Project.Name);

        return Task.CompletedTask;
    }
}