namespace Application.Domain.Projects;

public class ProjectSpecification
{
    public Guid Id { get; private set; }
    public int Version { get; private set; }
    public string Content { get; private set; } = null!;
    public bool IsApproved { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime ApprovedAt { get; private set; }

    public Guid ProjectId { get; private set; }
    public Project Project { get; private set; } = null!;

    private ProjectSpecification() { }
    public ProjectSpecification(int version, string content, Guid projectId)
    {
        Id = Guid.NewGuid();
        Version = version;
        Content = content;
        ProjectId = projectId;
        CreatedAt = DateTime.UtcNow;
        IsApproved = false;
    }

    public void Approve()
    {
        if (IsApproved) return;

        IsApproved = true;
        ApprovedAt = DateTime.UtcNow;
    }
}
