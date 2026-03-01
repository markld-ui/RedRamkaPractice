using Domain.Projects;
using FluentAssertions;

namespace Application.UnitTests.Domain;

/// <summary>
/// Тесты граничных случаев доменной логики, которые не покрыты другими классами.
/// Фокус: все пути кода через методы <see cref="Project"/>, которые
/// возвращают ошибки или бросают исключения.
/// </summary>
public class ProjectEdgeCaseTests
{
    private static Project CreateProject() => new("P", "D");

    private static Project CreateProjectAt(ProjectStage target)
    {
        var p = CreateProject();
        p.AddSpecification("s");
        p.ApproveSpecification(p.Specifications.First().Id);

        if (target == ProjectStage.Design) return p;

        p.StartDevelopment();
        if (target == ProjectStage.Development) return p;

        p.SendToQA();
        if (target == ProjectStage.QA) return p;

        p.PassQA();
        if (target == ProjectStage.Delivery) return p;

        p.Release();
        if (target == ProjectStage.Support) return p;

        p.Archive("done");
        return p; // Archived
    }

    // ─── StartDevelopment — все пути ошибок ──────────────────────────────────

    [Fact]
    public void StartDevelopment_WhenAlreadyInDevelopment_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Development);

        var result = project.StartDevelopment();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void StartDevelopment_WhenInQA_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.QA);

        var result = project.StartDevelopment();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void StartDevelopment_WhenArchived_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Archived);

        var result = project.StartDevelopment();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void StartDevelopment_WhenDesignWithRevokedSpec_ShouldFail()
    {
        var project = CreateProject();
        project.AddSpecification("s1");
        var spec1Id = project.Specifications.First().Id;
        project.ApproveSpecification(spec1Id);

        // Добавляем вторую и одобряем — первая отзывается
        project.AddSpecification("s2");
        project.ApproveSpecification(project.Specifications.Last().Id);

        // Теперь отзываем вторую вручную (через обходной путь)
        // Все спеки неутверждённые — StartDevelopment должен провалиться
        project.Specifications.Last().Revoke();

        var result = project.StartDevelopment();

        result.IsSuccess.Should().BeFalse();
    }

    // ─── SendToQA — все пути ошибок ───────────────────────────────────────────

    [Fact]
    public void SendToQA_WhenInDesign_ShouldFail()
    {
        var project = CreateProject();

        var result = project.SendToQA();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void SendToQA_WhenInQA_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.QA);

        var result = project.SendToQA();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void SendToQA_WhenArchived_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Archived);

        var result = project.SendToQA();

        result.IsSuccess.Should().BeFalse();
    }

    // ─── PassQA — все пути ошибок ─────────────────────────────────────────────

    [Fact]
    public void PassQA_WhenInDesign_ShouldFail()
    {
        var project = CreateProject();

        var result = project.PassQA();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PassQA_WhenInDevelopment_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Development);

        var result = project.PassQA();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void PassQA_WhenInDelivery_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Delivery);

        var result = project.PassQA();

        result.IsSuccess.Should().BeFalse();
    }

    // ─── FailQA — все пути ошибок ─────────────────────────────────────────────

    [Fact]
    public void FailQA_WhenInDesign_ShouldFail()
    {
        var project = CreateProject();

        var result = project.FailQA("bug");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void FailQA_WhenInDevelopment_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Development);

        var result = project.FailQA("bug");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void FailQA_WhenInSupport_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Support);

        var result = project.FailQA("bug");

        result.IsSuccess.Should().BeFalse();
    }

    // ─── Release — все пути ошибок ────────────────────────────────────────────

    [Fact]
    public void Release_WhenInDesign_ShouldFail()
    {
        var project = CreateProject();

        var result = project.Release();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Release_WhenInQA_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.QA);

        var result = project.Release();

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Release_WhenInSupport_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Support);

        var result = project.Release();

        result.IsSuccess.Should().BeFalse();
    }

    // ─── ReturnToDesign — все пути ошибок ────────────────────────────────────

    [Fact]
    public void ReturnToDesign_WhenInDesign_ShouldFail()
    {
        var project = CreateProject();

        var result = project.ReturnToDesign("reason");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ReturnToDesign_WhenInDevelopment_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Development);

        var result = project.ReturnToDesign("reason");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ReturnToDesign_WhenInQA_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.QA);

        var result = project.ReturnToDesign("reason");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void ReturnToDesign_WhenArchived_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Archived);

        var result = project.ReturnToDesign("reason");

        result.IsSuccess.Should().BeFalse();
    }

    // ─── Archive — все пути ошибок ────────────────────────────────────────────

    [Fact]
    public void Archive_WhenInDesign_ShouldFail()
    {
        var project = CreateProject();

        var result = project.Archive("reason");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Archive_WhenInDevelopment_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Development);

        var result = project.Archive("reason");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Archive_WhenInQA_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.QA);

        var result = project.Archive("reason");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Archive_WhenInDelivery_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Delivery);

        var result = project.Archive("reason");

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Archive_WhenAlreadyArchived_ShouldFail()
    {
        var project = CreateProjectAt(ProjectStage.Archived);

        var result = project.Archive("again");

        result.IsSuccess.Should().BeFalse();
    }

    // ─── FailQA — Reason передаётся в Transition ─────────────────────────────

    [Fact]
    public void FailQA_ShouldRecordReasonInTransition()
    {
        var project = CreateProjectAt(ProjectStage.QA);

        project.FailQA("Critical security bug");

        var lastTransition = project.Transitions.Last();
        lastTransition.Reason.Should().Be("Critical security bug");
    }

    [Fact]
    public void ReturnToDesign_ShouldRecordReasonInTransition()
    {
        var project = CreateProjectAt(ProjectStage.Support);

        project.ReturnToDesign("Needs full redesign");

        var lastTransition = project.Transitions.Last();
        lastTransition.Reason.Should().Be("Needs full redesign");
    }

    [Fact]
    public void Archive_ShouldRecordReasonInTransition()
    {
        var project = CreateProjectAt(ProjectStage.Support);

        project.Archive("Project completed");

        var lastTransition = project.Transitions.Last();
        lastTransition.Reason.Should().Be("Project completed");
    }

    // ─── Результаты содержат корректный NewStage ──────────────────────────────

    [Theory]
    [InlineData(ProjectStage.Development)]
    public void StartDevelopment_SuccessResult_ShouldContainDevelopmentStage(ProjectStage expected)
    {
        var project = CreateProject();
        project.AddSpecification("s");
        project.ApproveSpecification(project.Specifications.First().Id);

        var result = project.StartDevelopment();

        result.NewStage.Should().Be(expected);
    }

    [Fact]
    public void SendToQA_SuccessResult_ShouldContainQAStage()
    {
        var project = CreateProjectAt(ProjectStage.Development);

        var result = project.SendToQA();

        result.NewStage.Should().Be(ProjectStage.QA);
    }

    [Fact]
    public void PassQA_SuccessResult_ShouldContainDeliveryStage()
    {
        var project = CreateProjectAt(ProjectStage.QA);

        var result = project.PassQA();

        result.NewStage.Should().Be(ProjectStage.Delivery);
    }

    [Fact]
    public void FailQA_SuccessResult_ShouldContainDevelopmentStage()
    {
        var project = CreateProjectAt(ProjectStage.QA);

        var result = project.FailQA("bug");

        result.NewStage.Should().Be(ProjectStage.Development);
    }

    [Fact]
    public void Release_SuccessResult_ShouldContainSupportStage()
    {
        var project = CreateProjectAt(ProjectStage.Delivery);

        var result = project.Release();

        result.NewStage.Should().Be(ProjectStage.Support);
    }

    [Fact]
    public void ReturnToDesign_SuccessResult_ShouldContainDesignStage()
    {
        var project = CreateProjectAt(ProjectStage.Support);

        var result = project.ReturnToDesign("revisit");

        result.NewStage.Should().Be(ProjectStage.Design);
    }

    [Fact]
    public void Archive_SuccessResult_ShouldContainArchivedStage()
    {
        var project = CreateProjectAt(ProjectStage.Support);

        var result = project.Archive("done");

        result.NewStage.Should().Be(ProjectStage.Archived);
    }

    // ─── AddSpecification — граничные случаи ─────────────────────────────────

    [Theory]
    [InlineData(ProjectStage.Development)]
    [InlineData(ProjectStage.QA)]
    [InlineData(ProjectStage.Delivery)]
    [InlineData(ProjectStage.Support)]
    public void AddSpecification_WhenNotDesignOrArchived_ShouldSucceed(ProjectStage stage)
    {
        var project = CreateProjectAt(stage);

        var act = () => project.AddSpecification("new spec");

        act.Should().NotThrow();
    }

    // ─── Множественные циклы ──────────────────────────────────────────────────

    [Fact]
    public void MultipleQAFailCycles_ShouldAllSucceed()
    {
        var project = CreateProject();
        project.AddSpecification("s");
        project.ApproveSpecification(project.Specifications.First().Id);
        project.StartDevelopment();

        for (int i = 0; i < 5; i++)
        {
            project.SendToQA().IsSuccess.Should().BeTrue();
            project.FailQA($"bug {i}").IsSuccess.Should().BeTrue();
        }

        project.Stage.Should().Be(ProjectStage.Development);
        project.Transitions.Should().HaveCount(11); // 1 start + 5 sendToQA + 5 fail
    }

    [Fact]
    public void MultipleReturnToDesignCycles_ShouldAllSucceed()
    {
        for (int i = 0; i < 3; i++)
        {
            var project = CreateProjectAt(ProjectStage.Support);
            project.ReturnToDesign($"cycle {i}").IsSuccess.Should().BeTrue();
            project.Stage.Should().Be(ProjectStage.Design);
        }
    }
}