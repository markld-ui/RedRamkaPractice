using Application.Common.Interfaces;
using Application.Features.Credentials.Commands;
using Application.Features.Projects.Specifications.Queries;
using Application.Features.Roles.Commands;
using Application.Features.Roles.Queries;
using Application.Features.Users.Queries;
using Application.UnitTests.Common;
using Domain.Models;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application;

// ─── RevokeRefreshTokenCommandHandler ─────────────────────────────────────────

/// <summary>
/// Тесты обработчика <see cref="RevokeRefreshTokenCommandHandler"/>.
/// </summary>
public class RevokeRefreshTokenCommandHandlerTests : HandlerTestBase
{
    private RevokeRefreshTokenCommandHandler CreateHandler(
        IEnumerable<Credentials>? credentials = null)
    {
        var context = CreateContextMock(credentials: credentials);
        return new RevokeRefreshTokenCommandHandler(context.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrow()
    {
        var handler = CreateHandler(credentials: Enumerable.Empty<Credentials>());

        var act = async () => await handler.Handle(
            new RevokeRefreshTokenCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldNullifyRefreshToken()
    {
        var userId = Guid.NewGuid();
        var creds = CreateCredentials(userId, refreshToken: "active-token");

        var contextMock = CreateContextMock(credentials: new[] { creds });
        var handler = new RevokeRefreshTokenCommandHandler(contextMock.Object);

        await handler.Handle(new RevokeRefreshTokenCommand(userId), CancellationToken.None);

        creds.RefreshToken.Should().BeNull();
        contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenTokenAlreadyNull_ShouldCompleteWithoutError()
    {
        var userId = Guid.NewGuid();
        var creds = CreateCredentials(userId, refreshToken: null);

        var handler = CreateHandler(credentials: new[] { creds });

        var act = async () => await handler.Handle(
            new RevokeRefreshTokenCommand(userId),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
        creds.RefreshToken.Should().BeNull();
    }
}

// ─── AssignRoleToUserCommandHandler ───────────────────────────────────────────

/// <summary>
/// Тесты обработчика <see cref="AssignRoleToUserCommandHandler"/>.
/// </summary>
public class AssignRoleToUserCommandHandlerTests : HandlerTestBase
{
    private AssignRoleToUserCommandHandler CreateHandler(
        IEnumerable<UserRole>? userRoles = null)
    {
        var context = CreateContextMock(userRoles: userRoles);
        context.Setup(x => x.UserRoles.Add(It.IsAny<UserRole>()));
        return new AssignRoleToUserCommandHandler(context.Object);
    }

    [Fact]
    public async Task Handle_WhenAssignmentNotExists_ShouldAddAndSave()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var contextMock = CreateContextMock(userRoles: Enumerable.Empty<UserRole>());
        contextMock.Setup(x => x.UserRoles.Add(It.IsAny<UserRole>()));
        var handler = new AssignRoleToUserCommandHandler(contextMock.Object);

        await handler.Handle(new AssignRoleToUserCommand(userId, roleId), CancellationToken.None);

        contextMock.Verify(x => x.UserRoles.Add(It.Is<UserRole>(
            ur => ur.UserId == userId && ur.RoleId == roleId)), Times.Once);
        contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAssignmentAlreadyExists_ShouldSkip()
    {
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var existing = new UserRole { UserId = userId, RoleId = roleId };

        var contextMock = CreateContextMock(userRoles: new[] { existing });
        var handler = new AssignRoleToUserCommandHandler(contextMock.Object);

        await handler.Handle(new AssignRoleToUserCommand(userId, roleId), CancellationToken.None);

        contextMock.Verify(x => x.UserRoles.Add(It.IsAny<UserRole>()), Times.Never);
        contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SameUserDifferentRoles_ShouldAddBoth()
    {
        var userId = Guid.NewGuid();
        var roleId1 = Guid.NewGuid();
        var roleId2 = Guid.NewGuid();

        var existing = new UserRole { UserId = userId, RoleId = roleId1 };
        var contextMock = CreateContextMock(userRoles: new[] { existing });
        contextMock.Setup(x => x.UserRoles.Add(It.IsAny<UserRole>()));
        var handler = new AssignRoleToUserCommandHandler(contextMock.Object);

        // Вторая роль не существует — должна добавиться
        await handler.Handle(new AssignRoleToUserCommand(userId, roleId2), CancellationToken.None);

        contextMock.Verify(x => x.UserRoles.Add(It.Is<UserRole>(
            ur => ur.RoleId == roleId2)), Times.Once);
    }
}

// ─── CreateRoleCommandHandler ─────────────────────────────────────────────────

/// <summary>
/// Тесты обработчика <see cref="CreateRoleCommandHandler"/>.
/// </summary>
public class CreateRoleCommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_ShouldAddRoleAndReturnId()
    {
        var contextMock = CreateContextMock();
        contextMock.Setup(x => x.Roles.Add(It.IsAny<Role>()));
        var handler = new CreateRoleCommandHandler(contextMock.Object);

        var result = await handler.Handle(
            new CreateRoleCommand("QA Lead", "Leads quality assurance"),
            CancellationToken.None);

        result.Should().NotBeEmpty();
        contextMock.Verify(x => x.Roles.Add(It.Is<Role>(r =>
            r.Name == "QA Lead" &&
            r.Description == "Leads quality assurance")), Times.Once);
        contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnRoleIdFromCreatedRole()
    {
        Role? capturedRole = null;
        var contextMock = CreateContextMock();
        contextMock.Setup(x => x.Roles.Add(It.IsAny<Role>()))
            .Callback<Role>(r => capturedRole = r);
        var handler = new CreateRoleCommandHandler(contextMock.Object);

        var result = await handler.Handle(
            new CreateRoleCommand("Architect", "Solution architect"),
            CancellationToken.None);

        result.Should().Be(capturedRole!.Id);
    }
}

// ─── DeleteRoleCommandHandler ─────────────────────────────────────────────────

/// <summary>
/// Тесты обработчика <see cref="DeleteRoleCommandHandler"/>.
/// </summary>
public class DeleteRoleCommandHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_WhenRoleNotFound_ShouldThrow()
    {
        var context = CreateContextMock(roles: Enumerable.Empty<Role>());
        var handler = new DeleteRoleCommandHandler(context.Object);

        var act = async () => await handler.Handle(
            new DeleteRoleCommand(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenRoleExists_ShouldRemoveAndSave()
    {
        var role = CreateRole();
        var contextMock = CreateContextMock(roles: new[] { role });
        contextMock.Setup(x => x.Roles.Remove(It.IsAny<Role>()));
        var handler = new DeleteRoleCommandHandler(contextMock.Object);

        await handler.Handle(new DeleteRoleCommand(role.Id), CancellationToken.None);

        contextMock.Verify(x => x.Roles.Remove(role), Times.Once);
        contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}

// ─── GetAllRolesQueryHandler ──────────────────────────────────────────────────

/// <summary>
/// Тесты обработчика <see cref="GetAllRolesQueryHandler"/>.
/// </summary>
public class GetAllRolesQueryHandlerTests : HandlerTestBase
{
    [Fact]
    public async Task Handle_WhenNoRoles_ShouldReturnEmptyList()
    {
        var context = CreateContextMock(roles: Enumerable.Empty<Role>());
        var handler = new GetAllRolesQueryHandler(context.Object);

        var result = await handler.Handle(new GetAllRolesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRolesExist_ShouldReturnAll()
    {
        var roles = new[]
        {
            CreateRole(name: "Admin"),
            CreateRole(name: "Developer"),
            CreateRole(name: "Tester")
        };
        var context = CreateContextMock(roles: roles);
        var handler = new GetAllRolesQueryHandler(context.Object);

        var result = await handler.Handle(new GetAllRolesQuery(), CancellationToken.None);

        result.Should().HaveCount(3);
        result.Select(r => r.Name).Should().BeEquivalentTo("Admin", "Developer", "Tester");
    }
}

// ─── GetUserByIdQueryHandler ──────────────────────────────────────────────────

/// <summary>
/// Тесты обработчика <see cref="GetUserByIdQueryHandler"/>.
/// </summary>
public class GetUserByIdQueryHandlerTests : HandlerTestBase
{
    private GetUserByIdQueryHandler CreateHandler(IEnumerable<User>? users = null)
    {
        var context = CreateContextMock(users: users);
        return new GetUserByIdQueryHandler(context.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrow()
    {
        var handler = CreateHandler(users: Enumerable.Empty<User>());

        var act = async () => await handler.Handle(
            new GetUserByIdQuery(Guid.NewGuid()),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenUserFound_ShouldReturnCorrectDto()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, "Bob", "Jones");
        user.Credentials = CreateCredentials(userId, "bob@test.com");
        user.UserRoles = new List<UserRole>
        {
            new() { Role = CreateRole(name: "Developer") }
        };

        var handler = CreateHandler(users: new[] { user });

        var result = await handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("Bob");
        result.LastName.Should().Be("Jones");
        result.Email.Should().Be("bob@test.com");
        result.Roles.Should().Contain("Developer");
    }

    [Fact]
    public async Task Handle_UserWithMultipleRoles_ShouldReturnAllRoles()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);
        user.Credentials = CreateCredentials(userId);
        user.UserRoles = new List<UserRole>
        {
            new() { Role = CreateRole(name: "Developer") },
            new() { Role = CreateRole(name: "Tester") }
        };

        var handler = CreateHandler(users: new[] { user });

        var result = await handler.Handle(new GetUserByIdQuery(userId), CancellationToken.None);

        result.Roles.Should().BeEquivalentTo("Developer", "Tester");
    }
}

// ─── GetAllUsersQueryHandler ──────────────────────────────────────────────────

/// <summary>
/// Тесты обработчика <see cref="GetAllUsersQueryHandler"/>.
/// </summary>
public class GetAllUsersQueryHandlerTests : HandlerTestBase
{
    private GetAllUsersQueryHandler CreateHandler(IEnumerable<User>? users = null)
    {
        var context = CreateContextMock(users: users);
        return new GetAllUsersQueryHandler(context.Object);
    }

    [Fact]
    public async Task Handle_WhenNoUsers_ShouldReturnEmptyList()
    {
        var handler = CreateHandler(users: Enumerable.Empty<User>());

        var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapAllUsersToDto()
    {
        var u1 = CreateUser(firstName: "Alice", lastName: "A");
        u1.Credentials = CreateCredentials(u1.Id, "alice@test.com");
        u1.UserRoles = new List<UserRole>();

        var u2 = CreateUser(firstName: "Bob", lastName: "B");
        u2.Credentials = CreateCredentials(u2.Id, "bob@test.com");
        u2.UserRoles = new List<UserRole>
        {
            new() { Role = CreateRole(name: "Admin") }
        };

        var handler = CreateHandler(users: new[] { u1, u2 });

        var result = await handler.Handle(new GetAllUsersQuery(), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().ContainSingle(u => u.Email == "alice@test.com");
        result.Should().ContainSingle(u =>
            u.Email == "bob@test.com" && u.Roles.Contains("Admin"));
    }
}

// ─── GetSpecificationByIdQueryHandler ────────────────────────────────────────

/// <summary>
/// Тесты обработчика <see cref="GetSpecificationByIdQueryHandler"/>.
/// </summary>
public class GetSpecificationByIdQueryHandlerTests : HandlerTestBase
{
    private readonly Mock<IProjectAuthorizationService> _authMock = new();

    private GetSpecificationByIdQueryHandler CreateHandler(
        IEnumerable<ProjectSpecification>? specs = null)
    {
        var context = CreateContextMock(specs: specs);
        return new GetSpecificationByIdQueryHandler(context.Object, _authMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNotMember_ShouldThrowUnauthorized()
    {
        _authMock.Setup(x => x.RequireProjectMemberAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var handler = CreateHandler();
        var query = new GetSpecificationByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.Handle(query, CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenSpecNotFound_ShouldReturnNull()
    {
        _authMock.Setup(x => x.RequireProjectMemberAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(specs: Enumerable.Empty<ProjectSpecification>());
        var query = new GetSpecificationByIdQuery(Guid.NewGuid(), Guid.NewGuid());

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WhenSpecExistsForProject_ShouldReturnDto()
    {
        var projectId = Guid.NewGuid();
        var spec = new ProjectSpecification(1, "spec content", projectId);

        _authMock.Setup(x => x.RequireProjectMemberAsync(
            projectId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(specs: new[] { spec });
        var query = new GetSpecificationByIdQuery(projectId, spec.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(spec.Id);
        result.Version.Should().Be(1);
        result.Content.Should().Be("spec content");
        result.IsApproved.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenSpecBelongsToDifferentProject_ShouldReturnNull()
    {
        // Защита от подмены: spec.ProjectId не совпадает с query.ProjectId
        var realProjectId = Guid.NewGuid();
        var otherProjectId = Guid.NewGuid();
        var spec = new ProjectSpecification(1, "content", otherProjectId);

        _authMock.Setup(x => x.RequireProjectMemberAsync(
            realProjectId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(specs: new[] { spec });
        var query = new GetSpecificationByIdQuery(realProjectId, spec.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ApprovedSpec_ShouldReturnApprovedAt()
    {
        var projectId = Guid.NewGuid();
        var spec = new ProjectSpecification(1, "content", projectId);
        spec.Approve();

        _authMock.Setup(x => x.RequireProjectMemberAsync(
            projectId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(specs: new[] { spec });
        var query = new GetSpecificationByIdQuery(projectId, spec.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        result!.IsApproved.Should().BeTrue();
        result.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_UnapprovedSpec_ShouldReturnNullApprovedAt()
    {
        var projectId = Guid.NewGuid();
        var spec = new ProjectSpecification(1, "content", projectId);

        _authMock.Setup(x => x.RequireProjectMemberAsync(
            projectId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(specs: new[] { spec });
        var query = new GetSpecificationByIdQuery(projectId, spec.Id);

        var result = await handler.Handle(query, CancellationToken.None);

        result!.ApprovedAt.Should().BeNull();
    }
}