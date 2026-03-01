using Application.Common.Interfaces;
using Application.Features.Auth.Commands.Register;
using Application.UnitTests.Common;
using Domain.Models;
using FluentAssertions;
using Moq;

namespace Application.UnitTests.Application.Auth;

/// <summary>
/// Тесты обработчика <see cref="RegisterCommandHandler"/>.
/// </summary>
public class RegisterCommandHandlerTests : HandlerTestBase
{
    private readonly Mock<IPasswordHasher> _hasherMock = new();

    private RegisterCommandHandler CreateHandler(
        IEnumerable<Credentials>? credentials = null)
    {
        var contextMock = CreateContextMock(credentials: credentials);
        contextMock.Setup(x => x.Users.Add(It.IsAny<User>()));
        return new RegisterCommandHandler(contextMock.Object, _hasherMock.Object);
    }

    [Fact]
    public async Task Handle_WithNewEmail_ShouldReturnNewUserId()
    {
        // Arrange
        _hasherMock.Setup(x => x.Hash("password")).Returns("hashed");
        var handler = CreateHandler(credentials: Enumerable.Empty<Credentials>());
        var command = new RegisterCommand("John", "Doe", "new@test.com", "password");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldThrow()
    {
        // Arrange
        var existingCreds = CreateCredentials(Guid.NewGuid(), "existing@test.com");
        var handler = CreateHandler(credentials: new[] { existingCreds });
        var command = new RegisterCommand("Jane", "Doe", "existing@test.com", "password");

        // Act
        var act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        // Arrange
        _hasherMock.Setup(x => x.Hash("mypassword")).Returns("hashed_pass");
        var handler = CreateHandler(credentials: Enumerable.Empty<Credentials>());
        var command = new RegisterCommand("John", "Doe", "new@test.com", "mypassword");

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _hasherMock.Verify(x => x.Hash("mypassword"), Times.Once);
    }
}