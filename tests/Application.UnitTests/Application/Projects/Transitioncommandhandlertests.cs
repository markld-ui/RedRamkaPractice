using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Features.Projects.Commands;
using Application.Features.Projects.Commands.Shared;
using Application.UnitTests.Common;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Projects;

/// <summary>
/// Общие тесты для всех command-обработчиков переходов между стадиями.
/// Проверяет единообразные контракты: аутентификация, поиск проекта,
/// успешный и неуспешный результат.
/// </summary>

// ─── FailQACommandHandler ─────────────────────────────────────────────────────

public class FailQACommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private FailQACommandHandler CreateHandler(IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        context.Setup(x => x.ProjectTransitions.Add(It.IsAny<ProjectTransition>()));
        return new FailQACommandHandler(context.Object, _currentUserMock.Object, _authMock.Object);
    }

    private Project CreateProjectInQA()
    {
        var p = new Project("T", "D");
        p.AddSpecification("s");
        p.ApproveSpecification(p.Specifications.First().Id);
        p.StartDevelopment();
        p.SendToQA();
        return p;
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrow()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var act = async () => await CreateHandler()
            .Handle(new FailQACommand(Guid.NewGuid(), "reason"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrow()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var act = async () => await CreateHandler(projects: Enumerable.Empty<Project>())
            .Handle(new FailQACommand(Guid.NewGuid(), "reason"), CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WhenNotInQA_ShouldReturnFailResult()
    {
        var project = new Project("T", "D"); // Design stage

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new FailQACommand(project.Id, "reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WhenInQA_ShouldReturnDevelopmentStage()
    {
        var project = CreateProjectInQA();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new FailQACommand(project.Id, "bug found"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.NewStage.Should().Be(ProjectStage.Development.ToString());
    }
}

// ─── PassQACommandHandler ─────────────────────────────────────────────────────

public class PassQACommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private PassQACommandHandler CreateHandler(IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        context.Setup(x => x.ProjectTransitions.Add(It.IsAny<ProjectTransition>()));
        return new PassQACommandHandler(context.Object, _currentUserMock.Object, _authMock.Object);
    }

    private Project CreateProjectInQA()
    {
        var p = new Project("T", "D");
        p.AddSpecification("s");
        p.ApproveSpecification(p.Specifications.First().Id);
        p.StartDevelopment();
        p.SendToQA();
        return p;
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrow()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var act = async () => await CreateHandler()
            .Handle(new PassQACommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenNotInQA_ShouldReturnFailResult()
    {
        var project = new Project("T", "D"); // Design

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new PassQACommand(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenInQA_ShouldReturnDeliveryStage()
    {
        var project = CreateProjectInQA();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new PassQACommand(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.NewStage.Should().Be(ProjectStage.Delivery.ToString());
    }
}

// ─── ReleaseCommandHandler ────────────────────────────────────────────────────

public class ReleaseCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private ReleaseCommandHandler CreateHandler(IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        context.Setup(x => x.ProjectTransitions.Add(It.IsAny<ProjectTransition>()));
        return new ReleaseCommandHandler(context.Object, _currentUserMock.Object, _authMock.Object);
    }

    private Project CreateProjectInDelivery()
    {
        var p = new Project("T", "D");
        p.AddSpecification("s");
        p.ApproveSpecification(p.Specifications.First().Id);
        p.StartDevelopment();
        p.SendToQA();
        p.PassQA();
        return p;
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrow()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var act = async () => await CreateHandler()
            .Handle(new ReleaseCommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenNotInDelivery_ShouldReturnFailResult()
    {
        var project = new Project("T", "D"); // Design

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new ReleaseCommand(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenInDelivery_ShouldReturnSupportStage()
    {
        var project = CreateProjectInDelivery();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new ReleaseCommand(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.NewStage.Should().Be(ProjectStage.Support.ToString());
    }
}

// ─── SendToQACommandHandler ───────────────────────────────────────────────────

public class SendToQACommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private SendToQACommandHandler CreateHandler(IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        context.Setup(x => x.ProjectTransitions.Add(It.IsAny<ProjectTransition>()));
        return new SendToQACommandHandler(context.Object, _currentUserMock.Object, _authMock.Object);
    }

    private Project CreateProjectInDevelopment()
    {
        var p = new Project("T", "D");
        p.AddSpecification("s");
        p.ApproveSpecification(p.Specifications.First().Id);
        p.StartDevelopment();
        return p;
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrow()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var act = async () => await CreateHandler()
            .Handle(new SendToQACommand(Guid.NewGuid()), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenNotInDevelopment_ShouldReturnFailResult()
    {
        var project = new Project("T", "D"); // Design — нельзя отправить на QA напрямую

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new SendToQACommand(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenInDevelopment_ShouldReturnQAStage()
    {
        var project = CreateProjectInDevelopment();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new SendToQACommand(project.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.NewStage.Should().Be(ProjectStage.QA.ToString());
    }
}

// ─── ReturnToDesignCommandHandler ─────────────────────────────────────────────

public class ReturnToDesignCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private ReturnToDesignCommandHandler CreateHandler(IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        context.Setup(x => x.ProjectTransitions.Add(It.IsAny<ProjectTransition>()));
        return new ReturnToDesignCommandHandler(context.Object, _currentUserMock.Object, _authMock.Object);
    }

    private Project CreateProjectInSupport()
    {
        var p = new Project("T", "D");
        p.AddSpecification("s");
        p.ApproveSpecification(p.Specifications.First().Id);
        p.StartDevelopment();
        p.SendToQA();
        p.PassQA();
        p.Release();
        return p;
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrow()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var act = async () => await CreateHandler()
            .Handle(new ReturnToDesignCommand(Guid.NewGuid(), "r"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenNotInSupport_ShouldReturnFailResult()
    {
        var project = new Project("T", "D"); // Design

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new ReturnToDesignCommand(project.Id, "reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenInSupport_ShouldReturnDesignStage()
    {
        var project = CreateProjectInSupport();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new ReturnToDesignCommand(project.Id, "revisit"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.NewStage.Should().Be(ProjectStage.Design.ToString());
    }
}

// ─── ArchiveCommandHandler ────────────────────────────────────────────────────

public class ArchiveCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private ArchiveCommandHandler CreateHandler(IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        context.Setup(x => x.ProjectTransitions.Add(It.IsAny<ProjectTransition>()));
        return new ArchiveCommandHandler(context.Object, _currentUserMock.Object, _authMock.Object);
    }

    private Project CreateProjectInSupport()
    {
        var p = new Project("T", "D");
        p.AddSpecification("s");
        p.ApproveSpecification(p.Specifications.First().Id);
        p.StartDevelopment();
        p.SendToQA();
        p.PassQA();
        p.Release();
        return p;
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrow()
    {
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var act = async () => await CreateHandler()
            .Handle(new ArchiveCommand(Guid.NewGuid(), "r"), CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenNotInSupport_ShouldReturnFailResult()
    {
        var project = new Project("T", "D"); // Design

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new ArchiveCommand(project.Id, "reason"), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenInSupport_ShouldReturnArchivedStage()
    {
        var project = CreateProjectInSupport();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var result = await CreateHandler(projects: new[] { project })
            .Handle(new ArchiveCommand(project.Id, "done"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.NewStage.Should().Be(ProjectStage.Archived.ToString());
    }

    [Fact]
    public async Task Handle_SuccessfulArchive_ShouldSaveChanges()
    {
        var project = CreateProjectInSupport();
        var contextMock = CreateContextMock(projects: new[] { project });
        contextMock.Setup(x => x.ProjectTransitions.Add(It.IsAny<ProjectTransition>()));

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var handler = new ArchiveCommandHandler(
            contextMock.Object, _currentUserMock.Object, _authMock.Object);

        await handler.Handle(new ArchiveCommand(project.Id, "done"), CancellationToken.None);

        contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}