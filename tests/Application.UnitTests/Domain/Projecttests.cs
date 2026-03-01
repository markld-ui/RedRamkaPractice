using Domain.Projects;
using FluentAssertions;

namespace Application.UnitTests.Domain;

/// <summary>
/// Тесты доменной сущности <see cref="Project"/>.
/// </summary>
public class ProjectTests
{
    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static Project CreateProject(string name = "Test Project")
        => new(name, "Description");

    private static Project CreateProjectWithApprovedSpec()
    {
        var project = CreateProject();
        project.AddSpecification("Spec content");
        var spec = project.Specifications.First();
        project.ApproveSpecification(spec.Id);
        return project;
    }

    // ─── Construction ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldSetInitialStageToDesign()
    {
        var project = CreateProject();

        project.Stage.Should().Be(ProjectStage.Design);
    }

    [Fact]
    public void Constructor_ShouldRaiseDomainEvent()
    {
        var project = CreateProject();

        project.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void Constructor_ShouldSetNameAndDescription()
    {
        var project = new Project("My Project", "My Description");

        project.Name.Should().Be("My Project");
        project.Description.Should().Be("My Description");
    }

    // ─── Specifications ───────────────────────────────────────────────────────

    [Fact]
    public void AddSpecification_ShouldIncrementVersion()
    {
        var project = CreateProject();

        project.AddSpecification("First");
        project.AddSpecification("Second");

        project.Specifications.Should().HaveCount(2);
        project.Specifications.Max(s => s.Version).Should().Be(2);
    }

    [Fact]
    public void AddSpecification_WhenArchived_ShouldThrow()
    {
        var project = CreateProjectWithApprovedSpec();
        project.StartDevelopment();
        project.SendToQA();
        project.PassQA();
        project.Release();
        project.Archive("reason");

        var act = () => project.AddSpecification("new spec");

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*archived*");
    }

    [Fact]
    public void ApproveSpecification_ShouldSetIsApprovedTrue()
    {
        var project = CreateProject();
        project.AddSpecification("content");
        var specId = project.Specifications.First().Id;

        project.ApproveSpecification(specId);

        project.Specifications.First().IsApproved.Should().BeTrue();
    }

    [Fact]
    public void ApproveSpecification_ShouldRevokePreviousApproved()
    {
        var project = CreateProject();
        project.AddSpecification("first");
        project.AddSpecification("second");

        var firstId = project.Specifications.First().Id;
        var secondId = project.Specifications.Last().Id;

        project.ApproveSpecification(firstId);
        project.ApproveSpecification(secondId);

        project.Specifications.First(s => s.Id == firstId).IsApproved.Should().BeFalse();
        project.Specifications.First(s => s.Id == secondId).IsApproved.Should().BeTrue();
    }

    [Fact]
    public void ApproveSpecification_WhenAlreadyApproved_ShouldThrow()
    {
        var project = CreateProject();
        project.AddSpecification("content");
        var specId = project.Specifications.First().Id;
        project.ApproveSpecification(specId);

        var act = () => project.ApproveSpecification(specId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already approved*");
    }

    [Fact]
    public void ApproveSpecification_WhenNotFound_ShouldThrow()
    {
        var project = CreateProject();

        var act = () => project.ApproveSpecification(Guid.NewGuid());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    // ─── Members ──────────────────────────────────────────────────────────────

    [Fact]
    public void AddMember_ShouldAddMemberToProject()
    {
        var project = CreateProject();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        project.AddMember(userId, roleId);

        project.Members.Should().ContainSingle(m => m.UserId == userId);
    }

    [Fact]
    public void AddMember_WhenDuplicate_ShouldThrow()
    {
        var project = CreateProject();
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        project.AddMember(userId, roleId);
        var act = () => project.AddMember(userId, roleId);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already assigned*");
    }

    // ─── Transitions ──────────────────────────────────────────────────────────

    [Fact]
    public void StartDevelopment_WithoutApprovedSpec_ShouldFail()
    {
        var project = CreateProject();

        var result = project.StartDevelopment();

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("specification");
    }

    [Fact]
    public void StartDevelopment_WithApprovedSpec_ShouldSucceed()
    {
        var project = CreateProjectWithApprovedSpec();

        var result = project.StartDevelopment();

        result.IsSuccess.Should().BeTrue();
        project.Stage.Should().Be(ProjectStage.Development);
    }

    [Fact]
    public void FullCycle_Design_To_Archived_ShouldSucceed()
    {
        var project = CreateProjectWithApprovedSpec();

        project.StartDevelopment().IsSuccess.Should().BeTrue();
        project.SendToQA().IsSuccess.Should().BeTrue();
        project.PassQA().IsSuccess.Should().BeTrue();
        project.Release().IsSuccess.Should().BeTrue();
        project.Archive("done").IsSuccess.Should().BeTrue();

        project.Stage.Should().Be(ProjectStage.Archived);
        project.ArchivedAt.Should().NotBeNull();
    }

    [Fact]
    public void FailQA_ShouldReturnToDevelopment()
    {
        var project = CreateProjectWithApprovedSpec();
        project.StartDevelopment();
        project.SendToQA();

        var result = project.FailQA("bug found");

        result.IsSuccess.Should().BeTrue();
        project.Stage.Should().Be(ProjectStage.Development);
    }

    [Fact]
    public void ReturnToDesign_ShouldReturnFromSupport()
    {
        var project = CreateProjectWithApprovedSpec();
        project.StartDevelopment();
        project.SendToQA();
        project.PassQA();
        project.Release();

        var result = project.ReturnToDesign("needs rework");

        result.IsSuccess.Should().BeTrue();
        project.Stage.Should().Be(ProjectStage.Design);
    }

    [Fact]
    public void Transition_FromInvalidStage_ShouldFail()
    {
        var project = CreateProject();

        // Попытка отправить на QA из Design
        var result = project.SendToQA();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Archive_ShouldSetArchivedAt()
    {
        var project = CreateProjectWithApprovedSpec();
        project.StartDevelopment();
        project.SendToQA();
        project.PassQA();
        project.Release();

        var before = DateTime.UtcNow;
        project.Archive("archived");

        project.ArchivedAt.Should().NotBeNull();
        project.ArchivedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Transitions_ShouldRecordHistory()
    {
        var project = CreateProjectWithApprovedSpec();
        project.StartDevelopment();
        project.SendToQA();

        project.Transitions.Should().HaveCount(2);
        project.Transitions.Last().ToStage.Should().Be(ProjectStage.QA);
    }
}