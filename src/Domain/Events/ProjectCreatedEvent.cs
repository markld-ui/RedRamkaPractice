using Domain.Common;
using Domain.Projects;

namespace Domain.Events;

public class ProjectCreatedEvent : BaseEvent
{
    public Project Project { get; }

    public ProjectCreatedEvent(Project project)
    {
        Project = project ?? throw new ArgumentNullException(nameof(project));
    }
}