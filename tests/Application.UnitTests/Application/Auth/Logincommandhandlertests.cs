using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Login;
using Application.Features.Auth.DTO;
using Application.UnitTests.Common;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Auth;

/// <summary>
/// Тесты обработчика <see cref="LoginCommandHandler"/>.
/// </summary>
public class LoginCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<IPasswordHasher> _hasherMock = new();
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private LoginCommandHandler CreateHandler(
        IEnumerable<Credentials>? credentials = null,
        IEnumerable<User>? users = null)
    {
        var context = CreateContextMock(
            users: users,
            credentials: credentials);

        return new LoginCommandHandler(
            context.Object,
            _hasherMock.Object,
            _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnAuthResponse()
    {
        // Arrange
        var user = CreateUser();
        user.UserRoles = new List<UserRole>();

        var creds = CreateCredentials(user.Id, "test@test.com", "hash");
        creds.User = user;

        _hasherMock.Setup(x => x.Verify("password", "hash")).Returns(true);
        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns("access-token");

        var handler = CreateHandler(
            credentials: new[] { creds },
            users: new[] { user });

        var command = new LoginCommand("test@test.com", "password");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("access-token");
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithInvalidEmail_ShouldThrowUnauthorized()
    {
        // Arrange
        var handler = CreateHandler(credentials: Enumerable.Empty<Credentials>());
        var command = new LoginCommand("notfound@test.com", "password");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ShouldThrowUnauthorized()
    {
        // Arrange
        var user = CreateUser();
        var creds = CreateCredentials(user.Id, "test@test.com", "hash");
        creds.User = user;

        _hasherMock.Setup(x => x.Verify("wrong", "hash")).Returns(false);

        var handler = CreateHandler(credentials: new[] { creds });
        var command = new LoginCommand("test@test.com", "wrong");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}