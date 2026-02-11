namespace Application.Domain.Projects;

public class Project
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Name { get; private set; } = String.Empty;
    public string Description { get; private set; } = String.Empty;

    public ProjectStage Stage { get; private set; } = ProjectStage.Design;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    public ICollection<ProjectTransition> Transitions { get; private set; } =
        new List<ProjectTransition>();

    public ICollection<ProjectSpecification> Specifications { get; private set; } =
        new List<ProjectSpecification>();

    public ICollection<ProjectMember> Members { get; private set; } =
        new List<ProjectMember>();

    private ProjectStateMachine CreateStateMachine() =>
        new ProjectStateMachine(() => Stage, s => Stage = s);

    private void AddTransition(
        ProjectStage from,
        ProjectStage to,
        string? reason)
    {
        Transitions.Add(new ProjectTransition
        {
            FromStage = from,
            ToStage = to,
            Reason = reason,
            ChangedAt = DateTime.UtcNow,
            ProjectId = Id
        });

        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasApprovedSpecification() =>
        Specifications.Any(s => s.IsApproved);

    // -------------------------
    // FSM transitions
    // -------------------------

    public ProjectTransitionResult StartDevelopment()
    {
        if (Stage == ProjectStage.Archived)
            return ProjectTransitionResult.Fail("Project is archived");

        if (!HasApprovedSpecification())
            return ProjectTransitionResult.Fail("Approved specification is required to start development");

        return Fire(ProjectTrigger.StartDevelopment, null);
    }

    public ProjectTransitionResult SendToQA()
    {
        return Fire(ProjectTrigger.SendToQA, null);
    }

    public ProjectTransitionResult FailQA(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return ProjectTransitionResult.Fail("failure reason is required");

        return Fire(ProjectTrigger.FailQA, reason);
    }

    public ProjectTransitionResult PassQA()
    {
        return Fire(ProjectTrigger.PassQA, null);
    }

    public ProjectTransitionResult Release()
    {
        return Fire(ProjectTrigger.Release, null);
    }

    public ProjectTransitionResult ReturnToDesign(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return ProjectTransitionResult.Fail("Reason is required");

        return Fire(ProjectTrigger.ReturnToDesign, reason);
    }

    public ProjectTransitionResult Archive(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            return ProjectTransitionResult.Fail("Archive reason is required");

        var result = Fire(ProjectTrigger.Archive, reason);

        if (result.IsSuccess)
            ArchivedAt = DateTime.UtcNow;

        return result;
    }

    private ProjectTransitionResult Fire(
        ProjectTrigger trigger,
        string? reason)
    {
        var stateMachine = CreateStateMachine();

        if (!stateMachine.CanFire(trigger))
            return ProjectTransitionResult.Fail(
                $"Transition '{trigger}' is not allowed from stage '{Stage}'");

        var from = Stage;

        if (reason is null)
            stateMachine.Fire(trigger);
        else
            FireWithReason(stateMachine, trigger, reason);

        AddTransition(from, Stage, reason);

        return ProjectTransitionResult.Success(Stage);
    }

    private static void FireWithReason(
        ProjectStateMachine stateMachine,
        ProjectTrigger trigger,
        string reason)
    {
        switch (trigger)
        {
            case ProjectTrigger.FailQA:
                stateMachine.FireFailQA(reason);
                break;

            case ProjectTrigger.ReturnToDesign:
                stateMachine.FireReturnToDesign(reason);
                break;

            case ProjectTrigger.Archive:
                stateMachine.FireArchive(reason);
                break;

            default:
                throw new InvalidOperationException($"Trigger '{trigger}' does not support parameters");
        }
    }
}
