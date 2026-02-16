using Stateless;

namespace Domain.Projects;

public class ProjectStateMachine
{
    private readonly StateMachine<ProjectStage, ProjectTrigger> _machine;

    private readonly StateMachine<ProjectStage, ProjectTrigger>
        .TriggerWithParameters<string> _failQa;

    private readonly StateMachine<ProjectStage, ProjectTrigger>
        .TriggerWithParameters<string> _returnToDesign;

    private readonly StateMachine<ProjectStage, ProjectTrigger>
        .TriggerWithParameters<string> _archive;

    public ProjectStateMachine(
        Func<ProjectStage> stateAccessor,
        Action<ProjectStage> stateMutator)
    {
        _machine = new StateMachine<ProjectStage, ProjectTrigger>(
            stateAccessor,
            stateMutator);

        _failQa = _machine.SetTriggerParameters<string>(ProjectTrigger.FailQA);
        _returnToDesign = _machine.SetTriggerParameters<string>(ProjectTrigger.ReturnToDesign);
        _archive = _machine.SetTriggerParameters<string>(ProjectTrigger.Archive);

        Configure();
    }

    private void Configure()
    {
        _machine.Configure(ProjectStage.Design)
            .Permit(ProjectTrigger.StartDevelopment, ProjectStage.Development);

        _machine.Configure(ProjectStage.Development)
            .Permit(ProjectTrigger.SendToQA, ProjectStage.QA);

        _machine.Configure(ProjectStage.QA)
            .Permit(ProjectTrigger.PassQA, ProjectStage.Delivery)
            .Permit(ProjectTrigger.FailQA, ProjectStage.Development);

        _machine.Configure(ProjectStage.Delivery)
            .Permit(ProjectTrigger.Release, ProjectStage.Support);

        _machine.Configure(ProjectStage.Support)
            .Permit(ProjectTrigger.ReturnToDesign, ProjectStage.Design)
            .Permit(ProjectTrigger.Archive, ProjectStage.Archived);

        _machine.Configure(ProjectStage.Archived);
    }

    public bool CanFire(ProjectTrigger trigger)
        => _machine.CanFire(trigger);

    public void Fire(ProjectTrigger trigger)
        => _machine.Fire(trigger);

    public void FireFailQA(string reason)
        => _machine.Fire(_failQa, reason);

    public void FireReturnToDesign(string reason)
        => _machine.Fire(_returnToDesign, reason);

    public void FireArchive(string reason)
        => _machine.Fire(_archive, reason);
}
