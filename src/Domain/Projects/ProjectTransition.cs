namespace Domain.Projects;

public class ProjectTransition
{
    private ProjectTransition() { } // EF

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

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }

    public ProjectStage FromStage { get; private set; }
    public ProjectStage ToStage { get; private set; }

    public string? Reason { get; private set; }

    public DateTime ChangedAt { get; private set; }

    public Project Project { get; private set; } = null!;
}
