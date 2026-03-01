using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Features.Projects.Commands;
using Application.UnitTests.Common;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Projects;

/// <summary>
/// Тесты обработчика <see cref="StartDevelopmentCommandHandler"/>.
/// </summary>
public class StartDevelopmentCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private StartDevelopmentCommandHandler CreateHandler(
        IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        return new StartDevelopmentCommandHandler(
            context.Object,
            _currentUserMock.Object,
            _authMock.Object);
    }

    private static Project CreateProjectWithApprovedSpec()
    {
        var project = new Project("Test", "Desc");
        project.AddSpecification("content");
        var specId = project.Specifications.First().Id;
        project.ApproveSpecification(specId);
        return project;
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrowUnauthorized()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var handler = CreateHandler();
        var command = new StartDevelopmentCommand(Guid.NewGuid());

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrow()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>(),
                RoleConstants.ProjectManager))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: Enumerable.Empty<Project>());
        var command = new StartDevelopmentCommand(Guid.NewGuid());

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WithoutApprovedSpec_ShouldReturnFailResult()
    {
        // Arrange
        var project = new Project("Test", "Desc"); // без утверждённой спецификации

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
                project.Id,
                It.IsAny<CancellationToken>(),
                RoleConstants.ProjectManager))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: new[] { project });
        var command = new StartDevelopmentCommand(project.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("specification");
    }

    [Fact]
    public async Task Handle_WithApprovedSpec_ShouldReturnSuccessResult()
    {
        // Arrange
        var project = CreateProjectWithApprovedSpec();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
                project.Id,
                It.IsAny<CancellationToken>(),
                RoleConstants.ProjectManager))
            .Returns(Task.CompletedTask);

        var contextMock = CreateContextMock(projects: new[] { project });
        contextMock.Setup(x => x.ProjectTransitions.Add(It.IsAny<ProjectTransition>()));

        var handler = new StartDevelopmentCommandHandler(
            contextMock.Object,
            _currentUserMock.Object,
            _authMock.Object);

        var command = new StartDevelopmentCommand(project.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.NewStage.Should().Be(ProjectStage.Development.ToString());
    }
}