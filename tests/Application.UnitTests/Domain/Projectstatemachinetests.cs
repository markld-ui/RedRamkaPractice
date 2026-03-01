using Domain.Projects;
using FluentAssertions;

namespace Application.UnitTests.Domain;

/// <summary>
/// Тесты конечного автомата <see cref="ProjectStateMachine"/>.
/// </summary>
public class ProjectStateMachineTests
{
    private ProjectStage _stage;

    private ProjectStateMachine CreateMachine(ProjectStage initial = ProjectStage.Design)
    {
        _stage = initial;
        return new ProjectStateMachine(() => _stage, s => _stage = s);
    }

    // ─── CanFire ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(ProjectStage.Design, ProjectTrigger.StartDevelopment, true)]
    [InlineData(ProjectStage.Design, ProjectTrigger.SendToQA, false)]
    [InlineData(ProjectStage.Development, ProjectTrigger.SendToQA, true)]
    [InlineData(ProjectStage.Development, ProjectTrigger.PassQA, false)]
    [InlineData(ProjectStage.QA, ProjectTrigger.PassQA, true)]
    [InlineData(ProjectStage.QA, ProjectTrigger.FailQA, true)]
    [InlineData(ProjectStage.QA, ProjectTrigger.Release, false)]
    [InlineData(ProjectStage.Delivery, ProjectTrigger.Release, true)]
    [InlineData(ProjectStage.Delivery, ProjectTrigger.Archive, false)]
    [InlineData(ProjectStage.Support, ProjectTrigger.Archive, true)]
    [InlineData(ProjectStage.Support, ProjectTrigger.ReturnToDesign, true)]
    [InlineData(ProjectStage.Archived, ProjectTrigger.StartDevelopment, false)]
    public void CanFire_ShouldReturnExpectedResult(
        ProjectStage stage,
        ProjectTrigger trigger,
        bool expected)
    {
        var machine = CreateMachine(stage);

        machine.CanFire(trigger).Should().Be(expected);
    }

    // ─── Transitions ──────────────────────────────────────────────────────────

    [Fact]
    public void Fire_StartDevelopment_ShouldTransitionToDevelopment()
    {
        var machine = CreateMachine(ProjectStage.Design);

        machine.Fire(ProjectTrigger.StartDevelopment);

        _stage.Should().Be(ProjectStage.Development);
    }

    [Fact]
    public void Fire_SendToQA_ShouldTransitionToQA()
    {
        var machine = CreateMachine(ProjectStage.Development);

        machine.Fire(ProjectTrigger.SendToQA);

        _stage.Should().Be(ProjectStage.QA);
    }

    [Fact]
    public void Fire_PassQA_ShouldTransitionToDelivery()
    {
        var machine = CreateMachine(ProjectStage.QA);

        machine.Fire(ProjectTrigger.PassQA);

        _stage.Should().Be(ProjectStage.Delivery);
    }

    [Fact]
    public void FireFailQA_ShouldTransitionToDevelopment()
    {
        var machine = CreateMachine(ProjectStage.QA);

        machine.FireFailQA("bugs found");

        _stage.Should().Be(ProjectStage.Development);
    }

    [Fact]
    public void Fire_Release_ShouldTransitionToSupport()
    {
        var machine = CreateMachine(ProjectStage.Delivery);

        machine.Fire(ProjectTrigger.Release);

        _stage.Should().Be(ProjectStage.Support);
    }

    [Fact]
    public void FireReturnToDesign_ShouldTransitionToDesign()
    {
        var machine = CreateMachine(ProjectStage.Support);

        machine.FireReturnToDesign("needs rework");

        _stage.Should().Be(ProjectStage.Design);
    }

    [Fact]
    public void FireArchive_ShouldTransitionToArchived()
    {
        var machine = CreateMachine(ProjectStage.Support);

        machine.FireArchive("project done");

        _stage.Should().Be(ProjectStage.Archived);
    }
}