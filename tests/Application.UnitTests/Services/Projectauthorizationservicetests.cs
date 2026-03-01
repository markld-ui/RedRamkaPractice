using API.Services;
using Application.Common.Constants;
using Application.Common.Interfaces;
using Application.UnitTests.Common;
using Domain.Models;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Services;

/// <summary>
/// Тесты сервиса <see cref="ProjectAuthorizationService"/>.
/// </summary>
public class ProjectAuthorizationServiceTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private ProjectAuthorizationService CreateService(
        IEnumerable<ProjectMember>? members = null,
        IEnumerable<Role>? roles = null)
    {
        var context = CreateContextMock(members: members, roles: roles);
        return new ProjectAuthorizationService(context.Object, _currentUserMock.Object);
    }

    // ─── IsAdminAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task IsAdminAsync_WhenUserIsAdmin_ShouldReturnTrue()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var result = await service.IsAdminAsync();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsAdminAsync_WhenUserIsNotAdmin_ShouldReturnFalse()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(false);

        var service = CreateService();

        // Act
        var result = await service.IsAdminAsync();

        // Assert
        result.Should().BeFalse();
    }

    // ─── RequireProjectMemberAsync ────────────────────────────────────────────

    [Fact]
    public async Task RequireProjectMemberAsync_WhenAdmin_ShouldNotThrow()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var act = async () => await service.RequireProjectMemberAsync(
            Guid.NewGuid(), CancellationToken.None);

        // Assert — Admin проходит без проверки членства
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RequireProjectMemberAsync_WhenMember_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        var member = new ProjectMember(projectId, userId, Guid.NewGuid());

        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(false);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var service = CreateService(members: new[] { member });

        // Act
        var act = async () => await service.RequireProjectMemberAsync(
            projectId, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RequireProjectMemberAsync_WhenNotMember_ShouldThrowUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(false);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var service = CreateService(members: Enumerable.Empty<ProjectMember>());

        // Act
        var act = async () => await service.RequireProjectMemberAsync(
            projectId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not a member*");
    }

    // ─── RequireProjectRoleAsync ──────────────────────────────────────────────

    [Fact]
    public async Task RequireProjectRoleAsync_WhenAdmin_ShouldNotThrow()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(true);

        var service = CreateService();

        // Act
        var act = async () => await service.RequireProjectRoleAsync(
            Guid.NewGuid(), CancellationToken.None, RoleConstants.ProjectManager);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RequireProjectRoleAsync_WhenUserHasAllowedRole_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var pmRole = CreateRole(name: RoleConstants.ProjectManager);
        var member = new ProjectMember(projectId, userId, pmRole.Id);

        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(false);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var service = CreateService(
            members: new[] { member },
            roles: new[] { pmRole });

        // Act
        var act = async () => await service.RequireProjectRoleAsync(
            projectId, CancellationToken.None, RoleConstants.ProjectManager);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RequireProjectRoleAsync_WhenNotMember_ShouldThrowUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(false);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var service = CreateService(
            members: Enumerable.Empty<ProjectMember>(),
            roles: Enumerable.Empty<Role>());

        // Act
        var act = async () => await service.RequireProjectRoleAsync(
            projectId, CancellationToken.None, RoleConstants.ProjectManager);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*not a member*");
    }

    [Fact]
    public async Task RequireProjectRoleAsync_WhenMemberWithWrongRole_ShouldThrowUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var devRole = CreateRole(name: RoleConstants.Developer);
        var member = new ProjectMember(projectId, userId, devRole.Id);

        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(false);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var service = CreateService(
            members: new[] { member },
            roles: new[] { devRole });

        // Act — требуем ProjectManager, а у пользователя Developer
        var act = async () => await service.RequireProjectRoleAsync(
            projectId, CancellationToken.None, RoleConstants.ProjectManager);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*following roles*");
    }

    [Fact]
    public async Task RequireProjectRoleAsync_WhenOneOfAllowedRoles_ShouldNotThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var testerRole = CreateRole(name: RoleConstants.Tester);
        var member = new ProjectMember(projectId, userId, testerRole.Id);

        _currentUserMock.Setup(x => x.IsInRoleAsync(RoleConstants.Admin))
            .ReturnsAsync(false);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var service = CreateService(
            members: new[] { member },
            roles: new[] { testerRole });

        // Act — допускаем как ProjectManager так и Tester
        var act = async () => await service.RequireProjectRoleAsync(
            projectId, CancellationToken.None,
            RoleConstants.ProjectManager,
            RoleConstants.Tester);

        // Assert
        await act.Should().NotThrowAsync();
    }
}