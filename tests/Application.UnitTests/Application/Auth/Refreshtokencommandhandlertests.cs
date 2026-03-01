using Application.Common.Interfaces;
using Application.Features.Auth.Commands.RefreshToken;
using Application.UnitTests.Common;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Auth;

/// <summary>
/// Тесты обработчика <see cref="RefreshTokenCommandHandler"/>.
/// </summary>
public class RefreshTokenCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<ITokenService> _tokenServiceMock = new();

    private RefreshTokenCommandHandler CreateHandler(
        IEnumerable<Credentials>? credentials = null)
    {
        var context = CreateContextMock(credentials: credentials);
        return new RefreshTokenCommandHandler(context.Object, _tokenServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var user = CreateUser();
        user.UserRoles = new List<UserRole>();

        var creds = CreateCredentials(user.Id, refreshToken: "valid-refresh");
        creds.User = user;

        _tokenServiceMock.Setup(x => x.GenerateAccessToken(user)).Returns("new-access");

        var handler = CreateHandler(credentials: new[] { creds });
        var command = new RefreshTokenCommand("valid-refresh");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.AccessToken.Should().Be("new-access");
        result.RefreshToken.Should().NotBe("valid-refresh"); // должен быть обновлён
    }

    [Fact]
    public async Task Handle_WithInvalidRefreshToken_ShouldThrowUnauthorized()
    {
        // Arrange
        var handler = CreateHandler(credentials: Enumerable.Empty<Credentials>());
        var command = new RefreshTokenCommand("invalid-token");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }
}