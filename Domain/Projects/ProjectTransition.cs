namespace Domain.Projects;

public class ProjectTransition
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ProjectStage FromStage { get; set; }
    public ProjectStage ToStage { get; set; }
    public string? Reason { get; set; }
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public Guid ProjectId { get; set; }
    public Project? Project { get; set; }
}
