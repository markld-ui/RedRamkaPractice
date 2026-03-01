using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.Features.Projects.Specifications.Commands;
using Application.Features.Projects.Specifications.Queries;
using Application.UnitTests.Common;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Projects;

/// <summary>
/// Тесты обработчика <see cref="CreateSpecificationCommandHandler"/>.
/// </summary>
public class CreateSpecificationCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private CreateSpecificationCommandHandler CreateHandler(
        IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        context.Setup(x => x.ProjectSpecifications.Add(It.IsAny<ProjectSpecification>()));
        return new CreateSpecificationCommandHandler(
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
        var command = new CreateSpecificationCommand(Guid.NewGuid(), "content");

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
            RoleConstants.ProjectManager)).Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: Enumerable.Empty<Project>());
        var command = new CreateSpecificationCommand(Guid.NewGuid(), "content");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnSpecificationWithVersion1()
    {
        // Arrange
        var project = new Project("Test", "Desc");

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            RoleConstants.ProjectManager)).Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: new[] { project });
        var command = new CreateSpecificationCommand(project.Id, "spec content") { ProjectId = project.Id };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Version.Should().Be(1);
        result.IsApproved.Should().BeFalse();
        result.Id.Should().NotBeEmpty();
    }
}

/// <summary>
/// Тесты обработчика <see cref="ApproveSpecificationCommandHandler"/>.
/// </summary>
public class ApproveSpecificationCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private ApproveSpecificationCommandHandler CreateHandler(
        IEnumerable<Project>? projects = null)
    {
        var context = CreateContextMock(projects: projects);
        return new ApproveSpecificationCommandHandler(
            context.Object,
            _currentUserMock.Object,
            _authMock.Object);
    }

    [Fact]
    public async Task Handle_WhenSpecNotFound_ShouldThrow()
    {
        // Arrange
        var project = new Project("Test", "Desc"); // без спецификаций

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            RoleConstants.ProjectManager)).Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: new[] { project });
        var command = new ApproveSpecificationCommand(project.Id, Guid.NewGuid());

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldReturnApprovedResult()
    {
        // Arrange
        var project = new Project("Test", "Desc");
        project.AddSpecification("content");
        var specId = project.Specifications.First().Id;

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _authMock.Setup(x => x.RequireProjectRoleAsync(
            project.Id, It.IsAny<CancellationToken>(),
            RoleConstants.ProjectManager)).Returns(Task.CompletedTask);

        var handler = CreateHandler(projects: new[] { project });
        var command = new ApproveSpecificationCommand(project.Id, specId);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsApproved.Should().BeTrue();
        result.Id.Should().Be(specId);
    }
}

/// <summary>
/// Тесты обработчика <see cref="GetProjectSpecificationsQueryHandler"/>.
/// </summary>
public class GetProjectSpecificationsQueryHandlerTests : HandlerTestBase
{
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private GetProjectSpecificationsQueryHandler CreateHandler(
        IEnumerable<Project>? projects = null,
        IEnumerable<ProjectSpecification>? specs = null)
    {
        var context = CreateContextMock(projects: projects, specs: specs);
        return new GetProjectSpecificationsQueryHandler(context.Object, _authMock.Object);
    }

    [Fact]
    public async Task Handle_WhenProjectNotFound_ShouldThrow()
    {
        // Arrange
        _authMock.Setup(x => x.RequireProjectMemberAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(
            projects: Enumerable.Empty<Project>(),
            specs: Enumerable.Empty<ProjectSpecification>());

        var query = new GetProjectSpecificationsQuery(Guid.NewGuid());

        // Act
        var act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ShouldReturnSpecsOrderedByVersion()
    {
        // Arrange
        var project = new Project("Test", "Desc");
        project.AddSpecification("v1");
        project.AddSpecification("v2");
        project.AddSpecification("v3");

        _authMock.Setup(x => x.RequireProjectMemberAsync(
            project.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var specs = project.Specifications.ToList();
        var handler = CreateHandler(
            projects: new[] { project },
            specs: specs);

        var query = new GetProjectSpecificationsQuery(project.Id);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Select(s => s.Version).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task Handle_ApprovedSpec_ShouldIncludeApprovedAt()
    {
        // Arrange
        var project = new Project("Test", "Desc");
        project.AddSpecification("content");
        var spec = project.Specifications.First();
        project.ApproveSpecification(spec.Id);

        _authMock.Setup(x => x.RequireProjectMemberAsync(
            project.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(
            projects: new[] { project },
            specs: project.Specifications.ToList());

        var query = new GetProjectSpecificationsQuery(project.Id);

        // Act
        var result = (await handler.Handle(query, CancellationToken.None)).ToList();

        // Assert
        result.Single().ApprovedAt.Should().NotBeNull();
    }
}