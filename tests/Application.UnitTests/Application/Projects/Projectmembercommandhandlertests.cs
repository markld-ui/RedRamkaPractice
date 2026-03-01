using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Features.Projects.Commands;
using Application.UnitTests.Common;
using Domain.Models;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Projects;

/// <summary>
/// Тесты обработчика <see cref="AddProjectMemberCommandHandler"/>.
/// </summary>
public class AddProjectMemberCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private AddProjectMemberCommandHandler CreateHandler(
        IEnumerable<Project>? projects = null,
        IEnumerable<User>? users = null,
        IEnumerable<Role>? roles = null)
    {
        var context = CreateContextMock(
            projects: projects,
            users: users,
            roles: roles);
        return new AddProjectMemberCommandHandler(
            context.Object,
            _currentUserMock.Object,
            _authMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrowUnauthorized()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var handler = CreateHandler();
        var command = new AddProjectMemberCommand(Guid.NewGuid(), Guid.NewGuid(), RoleConstants.Developer);

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
            It.IsAny<Guid>(), It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: Enumerable.Empty<Project>());
        var command = new AddProjectMemberCommand(Guid.NewGuid(), Guid.NewGuid(), RoleConstants.Developer);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrow()
    {
        // Arrange
        var project = new Project("Test", "Desc");

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var handler = CreateHandler(
            projects: new[] { project },
            users: Enumerable.Empty<User>());

        var command = new AddProjectMemberCommand(project.Id, Guid.NewGuid(), RoleConstants.Developer);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }
}

/// <summary>
/// Тесты обработчика <see cref="RemoveProjectMemberCommandHandler"/>.
/// </summary>
public class RemoveProjectMemberCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private RemoveProjectMemberCommandHandler CreateHandler(
        IEnumerable<Project>? projects = null,
        IEnumerable<Role>? roles = null)
    {
        var context = CreateContextMock(projects: projects, roles: roles);
        return new RemoveProjectMemberCommandHandler(
            context.Object,
            _currentUserMock.Object,
            _authMock.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotMember_ShouldThrow()
    {
        // Arrange
        var project = new Project("Test", "Desc");

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: new[] { project });
        var command = new RemoveProjectMemberCommand(project.Id, Guid.NewGuid());

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not a member*");
    }

    [Fact]
    public async Task Handle_RemovingLastProjectManager_ShouldThrow()
    {
        // Arrange
        var pmRole = CreateRole(name: RoleConstants.ProjectManager);
        var project = new Project("Test", "Desc");
        var userId = Guid.NewGuid();
        project.AddMember(userId, pmRole.Id);

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            It.IsAny<string[]>())).Returns(Task.CompletedTask);

        var handler = CreateHandler(
            projects: new[] { project },
            roles: new[] { pmRole });

        var command = new RemoveProjectMemberCommand(project.Id, userId);

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*last ProjectManager*");
    }
}