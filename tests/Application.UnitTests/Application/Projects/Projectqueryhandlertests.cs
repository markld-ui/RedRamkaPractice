using Application.Common.Interfaces;
using Application.Features.Projects.Queries;
using Application.UnitTests.Common;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Projects;

/// <summary>
/// Тесты обработчика <see cref="GetProjectByIdQueryHandler"/>.
/// </summary>
public class GetProjectByIdQueryHandlerTests : HandlerTestBase
{
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private GetProjectByIdQueryHandler CreateHandler(
        IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        return new GetProjectByIdQueryHandler(context.Object, _authMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldReturnNull()
    {
        // Arrange
        _authMock.Setup(x => x.RequireProjectMemberAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: Enumerable.Empty<Project>());
        var query = new GetProjectByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenProjectExists_ShouldReturnDto()
    {
        // Arrange
        var project = new Project("My Project", "Description");

        _authMock.Setup(x => x.RequireProjectMemberAsync(
            project.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: new[] { project });
        var query = new GetProjectByIdQuery(project.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(project.Id);
        result.Name.Should().Be("My Project");
        result.Stage.Should().Be(ProjectStage.Design.ToString());
    }

    [Fact]
    public async Task Handle_WhenUnauthorized_ShouldThrow()
    {
        // Arrange
        var project = new Project("My Project", "Description");

        _authMock.Setup(x => x.RequireProjectMemberAsync(
            project.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var handler = CreateHandler(projects: new[] { project });
        var query = new GetProjectByIdQuery(project.Id);

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}

/// <summary>
/// Тесты обработчика <see cref="GetProjectsQueryHandler"/>.
/// </summary>
public class GetProjectsQueryHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private GetProjectsQueryHandler CreateHandler(
        IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        return new GetProjectsQueryHandler(
            context.Object,
            _currentUserMock.Object,
            _authMock.Object);
    }

    [Fact]
    public async Task Handle_AsAdmin_ShouldReturnAllProjects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var p1 = new Project("Project 1", "Desc");
        var p2 = new Project("Project 2", "Desc");

        _authMock.Setup(x => x.IsAdminAsync()).ReturnsAsync(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var handler = CreateHandler(projects: new[] { p1, p2 });

        // Act
        var result = await handler.Handle(new GetProjectsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_AsRegularUser_ShouldReturnOnlyOwnProjects()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var ownProject = new Project("Own Project", "Desc");
        ownProject.AddMember(userId, roleId);

        var otherProject = new Project("Other Project", "Desc");

        _authMock.Setup(x => x.IsAdminAsync()).ReturnsAsync(false);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var handler = CreateHandler(projects: new[] { ownProject, otherProject });

        // Act
        var result = await handler.Handle(new GetProjectsQuery(), CancellationToken.None);

        // Assert
        result.Should().ContainSingle(p => p.Id == ownProject.Id);
    }
}