using API.Services;
using FluentAssertions;

namespace Application.UnitTests.Services;

/// <summary>
/// Тесты сервиса <see cref="PasswordHasher"/>.
/// </summary>
public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    // ─── Hash ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Hash_ShouldReturnNonEmptyString()
    {
        var result = _hasher.Hash("mypassword");

        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_ShouldNotReturnPlainTextPassword()
    {
        var result = _hasher.Hash("mypassword");

        result.Should().NotBe("mypassword");
    }

    [Fact]
    public void Hash_SamPasswordTwice_ShouldReturnDifferentHashes()
    {
        // BCrypt генерирует разную соль при каждом вызове
        var hash1 = _hasher.Hash("mypassword");
        var hash2 = _hasher.Hash("mypassword");

        hash1.Should().NotBe(hash2);
    }

    // ─── Verify ───────────────────────────────────────────────────────────────

    [Fact]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        var hash = _hasher.Hash("correctpassword");

        var result = _hasher.Verify("correctpassword", hash);

        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WithWrongPassword_ShouldReturnFalse()
    {
        var hash = _hasher.Hash("correctpassword");

        var result = _hasher.Verify("wrongpassword", hash);

        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_WithEmptyPassword_ShouldReturnFalse()
    {
        var hash = _hasher.Hash("correctpassword");

        var result = _hasher.Verify(string.Empty, hash);

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("password123")]
    [InlineData("P@ssw0rd!")]
    [InlineData("very long password with spaces and symbols !@#$%")]
    public void Hash_And_Verify_ShouldWorkForVariousPasswords(string password)
    {
        var hash = _hasher.Hash(password);

        _hasher.Verify(password, hash).Should().BeTrue();
    }
}