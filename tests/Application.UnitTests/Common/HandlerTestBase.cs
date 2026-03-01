using Application.Common.Interfaces;
using Domain.Models;
using Domain.Projects;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Application.UnitTests.Common;

/// <summary>
/// Базовый вспомогательный класс для тестов, использующих мок <see cref="IApplicationDbContext"/>.
/// Предоставляет фабричные методы для создания тестовых данных.
/// </summary>
public abstract class HandlerTestBase
{
    // ─── DbContext Mock ───────────────────────────────────────────────────────

    protected static Mock<IApplicationDbContext> CreateContextMock(
        IEnumerable<User>? users = null,
        IEnumerable<Role>? roles = null,
        IEnumerable<Credentials>? credentials = null,
        IEnumerable<UserRole>? userRoles = null,
        IEnumerable<Project>? projects = null,
        IEnumerable<ProjectMember>? members = null,
        IEnumerable<ProjectSpecification>? specs = null,
        IEnumerable<ProjectTransition>? transitions = null)
    {
        var mock = new Mock<IApplicationDbContext>();

        mock.Setup(x => x.Users).Returns(CreateDbSet(users));
        mock.Setup(x => x.Roles).Returns(CreateDbSet(roles));
        mock.Setup(x => x.Credentials).Returns(CreateDbSet(credentials));
        mock.Setup(x => x.UserRoles).Returns(CreateDbSet(userRoles));
        mock.Setup(x => x.Projects).Returns(CreateDbSet(projects));
        mock.Setup(x => x.ProjectMembers).Returns(CreateDbSet(members));
        mock.Setup(x => x.ProjectSpecifications).Returns(CreateDbSet(specs));
        mock.Setup(x => x.ProjectTransitions).Returns(CreateDbSet(transitions));

        mock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        return mock;
    }

    private static DbSet<T> CreateDbSet<T>(IEnumerable<T>? items) where T : class
    {
        var list = (items ?? Enumerable.Empty<T>()).AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IAsyncEnumerable<T>>()
            .Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<T>(list.GetEnumerator()));

        mockSet.As<IQueryable<T>>()
            .Setup(x => x.Provider)
            .Returns(new TestAsyncQueryProvider<T>(list.Provider));

        mockSet.As<IQueryable<T>>().Setup(x => x.Expression).Returns(list.Expression);
        mockSet.As<IQueryable<T>>().Setup(x => x.ElementType).Returns(list.ElementType);
        mockSet.As<IQueryable<T>>().Setup(x => x.GetEnumerator()).Returns(list.GetEnumerator());

        return mockSet.Object;
    }

    // ─── Test Data Factories ──────────────────────────────────────────────────

    protected static User CreateUser(
        Guid? id = null,
        string firstName = "John",
        string lastName = "Doe")
    {
        var userId = id ?? Guid.NewGuid();
        return new User
        {
            Id = userId,
            FirstName = firstName,
            LastName = lastName
        };
    }

    protected static Role CreateRole(
        Guid? id = null,
        string name = "Developer")
        => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            Description = $"{name} role"
        };

    protected static Credentials CreateCredentials(
        Guid userId,
        string email = "test@test.com",
        string passwordHash = "hash",
        string? refreshToken = null)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Email = email,
            PasswordHash = passwordHash,
            RefreshToken = refreshToken
        };
}