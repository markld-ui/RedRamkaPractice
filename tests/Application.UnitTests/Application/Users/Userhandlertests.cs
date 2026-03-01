using Application.Common.Interfaces;
using Application.Features.Users.Commands;
using Application.Features.Users.Queries;
using Application.UnitTests.Common;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Users;

/// <summary>
/// Тесты обработчика <see cref="GetCurrentUserQueryHandler"/>.
/// </summary>
public class GetCurrentUserQueryHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private GetCurrentUserQueryHandler CreateHandler(
        IEnumerable<User>? users = null)
    {
        var context = CreateContextMock(users: users);
        return new GetCurrentUserQueryHandler(context.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrowUnauthorized()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, "Alice", "Smith");
        user.Credentials = CreateCredentials(userId, "alice@test.com");
        user.UserRoles = new List<UserRole>();

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var handler = CreateHandler(users: new[] { user });

        // Act
        var result = await handler.Handle(new GetCurrentUserQuery(), CancellationToken.None);

        // Assert
        result.Id.Should().Be(userId);
        result.FirstName.Should().Be("Alice");
        result.Email.Should().Be("alice@test.com");
    }
}

/// <summary>
/// Тесты обработчика <see cref="UpdateUserCommandHandler"/>.
/// </summary>
public class UpdateUserCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private UpdateUserCommandHandler CreateHandler(IEnumerable<User>? users = null)
    {
        var context = CreateContextMock(users: users);
        return new UpdateUserCommandHandler(context.Object, _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrowUnauthorized()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var handler = CreateHandler();
        var command = new UpdateUserCommand("John", "Doe");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WhenAuthenticated_ShouldUpdateUserFields()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(userId, "OldName", "OldSurname");

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);

        var handler = CreateHandler(users: new[] { user });
        var command = new UpdateUserCommand("NewName", "NewSurname");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        user.FirstName.Should().Be("NewName");
        user.LastName.Should().Be("NewSurname");
    }
}

/// <summary>
/// Тесты обработчика <see cref="DeleteUserCommandHandler"/>.
/// </summary>
public class DeleteUserCommandHandlerTests : HandlerTestBase
{
    private DeleteUserCommandHandler CreateHandler(IEnumerable<User>? users = null)
    {
        var context = CreateContextMock(users: users);
        context.Setup(x => x.Users.Remove(It.IsAny<User>()));
        return new DeleteUserCommandHandler(context.Object);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldThrow()
    {
        // Arrange
        var handler = CreateHandler(users: Enumerable.Empty<User>());
        var command = new DeleteUserCommand(Guid.NewGuid());

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WhenUserExists_ShouldCallRemove()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = CreateUser(userId);

        var contextMock = CreateContextMock(users: new[] { user });
        contextMock.Setup(x => x.Users.Remove(It.IsAny<User>()));
        var handler = new DeleteUserCommandHandler(contextMock.Object);

        var command = new DeleteUserCommand(userId);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        contextMock.Verify(x => x.Users.Remove(user), Times.Once);
        contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}