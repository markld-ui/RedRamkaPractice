using Domain.Models;

namespace Domain.Projects;

public class ProjectMember
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid UserId { get; private set; }
    public Role Role { get; private set; }
    public DateTime JoinedAt { get; private set; }

    public Project Project { get; private set; } = null!;

    private ProjectMember() { } // EF Core

    public ProjectMember(Guid projectId, Guid userId, Role role)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        UserId = userId;
        Role = role;
        JoinedAt = DateTime.UtcNow;
    }
}

