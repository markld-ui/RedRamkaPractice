using Application.Common.Interfaces;
using Application.Features.Credentials.Commands;
using Application.UnitTests.Common;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Credential;

/// <summary>
/// Тесты обработчика <see cref="ChangePasswordCommandHandler"/>.
/// </summary>
public class ChangePasswordCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ICurrentUserService> _currentUserMock = new();

    private ChangePasswordCommandHandler CreateHandler(
        IEnumerable<Credentials>? credentials = null)
    {
        var context = CreateContextMock(credentials: credentials);
        return new ChangePasswordCommandHandler(
            context.Object,
            _hasherMock.Object,
            _currentUserMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ShouldThrowUnauthorized()
    {
        // Arrange
        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(false);
        var handler = CreateHandler();
        var command = new ChangePasswordCommand("old", "new");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WithWrongCurrentPassword_ShouldThrowUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var creds = CreateCredentials(userId, passwordHash: "correct-hash");

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _hasherMock.Setup(x => x.Verify("wrong", "correct-hash")).Returns(false);

        var handler = CreateHandler(credentials: new[] { creds });
        var command = new ChangePasswordCommand("wrong", "newpassword");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WithCorrectPassword_ShouldUpdateHashAndRevokeToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var creds = CreateCredentials(userId, passwordHash: "old-hash", refreshToken: "token");

        _currentUserMock.Setup(x => x.IsAuthenticated).Returns(true);
        _currentUserMock.Setup(x => x.UserId).Returns(userId);
        _hasherMock.Setup(x => x.Verify("oldpass", "old-hash")).Returns(true);
        _hasherMock.Setup(x => x.Hash("newpass")).Returns("new-hash");

        var handler = CreateHandler(credentials: new[] { creds });
        var command = new ChangePasswordCommand("oldpass", "newpass");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        creds.PasswordHash.Should().Be("new-hash");
        creds.RefreshToken.Should().BeNull();
    }
}