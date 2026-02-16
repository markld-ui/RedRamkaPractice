using Domain.Common;
using Domain.Events;

namespace Domain.Projects;

public class Project : BaseEntity
{
    private readonly List<ProjectTransition> _transitions = new();
    private readonly List<ProjectSpecification> _specifications = new();
    private readonly List<ProjectMember> _members = new();

    private Project() { } // EF

    public Project(string name, string description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Stage = ProjectStage.Design;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new ProjectCreatedEvent(this));
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public ProjectStage Stage { get; private set; }

    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ArchivedAt { get; private set; }

    public IReadOnlyCollection<ProjectTransition> Transitions => _transitions;
    public IReadOnlyCollection<ProjectSpecification> Specifications => _specifications;
    public IReadOnlyCollection<ProjectMember> Members => _members;

    private ProjectStateMachine CreateStateMachine()
        => new(() => Stage, s => Stage = s);

    private void AddTransition(ProjectStage from, ProjectStage to, string? reason)
    {
        _transitions.Add(new ProjectTransition(Id, from, to, reason));
        UpdatedAt = DateTime.UtcNow;
    }

    public bool HasApprovedSpecification()
        => _specifications.Any(s => s.IsApproved);

    public void AddMember(Guid userId, Guid roleId)
    {
        if (_members.Any(m => m.UserId == userId))
            throw new InvalidOperationException("User already assigned to project.");

        _members.Add(new ProjectMember(Id, userId, roleId));
    }

    public ProjectTransitionResult StartDevelopment()
    {
        if (!HasApprovedSpecification())
            return ProjectTransitionResult.Fail("No approved specification.");

        return Execute(ProjectTrigger.StartDevelopment);
    }

    public ProjectTransitionResult SendToQA()
        => Execute(ProjectTrigger.SendToQA);

    public ProjectTransitionResult PassQA()
        => Execute(ProjectTrigger.PassQA);

    public ProjectTransitionResult Release()
        => Execute(ProjectTrigger.Release);

    public ProjectTransitionResult FailQA(string reason)
        => Execute(ProjectTrigger.FailQA, reason);

    public ProjectTransitionResult ReturnToDesign(string reason)
        => Execute(ProjectTrigger.ReturnToDesign, reason);

    public ProjectTransitionResult Archive(string reason)
    {
        var result = Execute(ProjectTrigger.Archive, reason);

        if (result.IsSuccess)
            ArchivedAt = DateTime.UtcNow;

        return result;
    }

    private ProjectTransitionResult Execute(ProjectTrigger trigger, string? reason = null)
    {
        var machine = CreateStateMachine();
        var from = Stage;

        if (!machine.CanFire(trigger))
            return ProjectTransitionResult.Fail(
                $"Cannot execute {trigger} from {Stage}");

        switch (trigger)
        {
            case ProjectTrigger.FailQA:
                machine.FireFailQA(reason!);
                break;

            case ProjectTrigger.ReturnToDesign:
                machine.FireReturnToDesign(reason!);
                break;

            case ProjectTrigger.Archive:
                machine.FireArchive(reason!);
                break;

            default:
                machine.Fire(trigger);
                break;
        }

        AddTransition(from, Stage, reason);

        return ProjectTransitionResult.Success(Stage);
    }
}
