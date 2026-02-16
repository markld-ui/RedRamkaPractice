namespace Domain.Projects;

public class ProjectMember
{
    private ProjectMember() { } // EF

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

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    public DateTime JoinedAt { get; private set; }

    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; }

}
