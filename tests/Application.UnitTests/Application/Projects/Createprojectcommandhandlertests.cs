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
/// Тесты обработчика <see cref="CreateProjectCommandHandler"/>.
/// </summary>
public class CreateProjectCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private CreateProjectCommandHandler CreateHandler(
        IEnumerable<User>? users = null,
        IEnumerable<Role>? roles = null,
        IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(
            users: users,
            roles: roles,
            projects: projects);

        context.Setup(x => x.Projects.Add(It.IsAny<Project>()));

        return new CreateProjectCommandHandler(context.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrowUnauthorized()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var handler = CreateHandler();
        var command = new CreateProjectCommand("Project", "Desc", new List<Guid>());

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldThrow()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var creator = CreateUser(creatorId);
        var pmRole = CreateRole(name: RoleConstants.ProjectManager);
        var devRole = CreateRole(name: RoleConstants.Developer);
        var existing = new Project("Existing Project", "desc");

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(creatorId);

        var handler = CreateHandler(
            users: new[] { creator },
            roles: new[] { pmRole, devRole },
            projects: new[] { existing });

        var command = new CreateProjectCommand("Existing Project", "desc", new List<Guid>());

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_WithMissingUsers_ShouldThrow()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var creator = CreateUser(creatorId);
        var pmRole = CreateRole(name: RoleConstants.ProjectManager);
        var devRole = CreateRole(name: RoleConstants.Developer);

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(creatorId);

        var handler = CreateHandler(
            users: new[] { creator },
            roles: new[] { pmRole, devRole },
            projects: Enumerable.Empty<Project>());

        var nonExistentUserId = Guid.NewGuid();
        var command = new CreateProjectCommand(
            "New Project", "desc",
            new List<Guid> { nonExistentUserId });

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnResult()
    {
        // Arrange
        var creatorId = Guid.NewGuid();
        var creator = CreateUser(creatorId);
        var pmRole = CreateRole(name: RoleConstants.ProjectManager);
        var devRole = CreateRole(name: RoleConstants.Developer);

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(creatorId);

        var handler = CreateHandler(
            users: new[] { creator },
            roles: new[] { pmRole, devRole },
            projects: Enumerable.Empty<Project>());

        var command = new CreateProjectCommand("New Project", "desc", new List<Guid>());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Project");
        result.Stage.Should().Be(ProjectStage.Design);
        result.Id.Should().NotBeEmpty();
    }
}