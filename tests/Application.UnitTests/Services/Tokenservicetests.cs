using API.Services;
using Domain.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application.UnitTests.Services;

/// <summary>
/// Тесты сервиса <see cref="TokenService"/>.
/// </summary>
public class TokenServiceTests
{
    private static TokenService CreateService(
        string key = "super-secret-key-that-is-long-enough-for-hmac",
        string issuer = "TestIssuer",
        string audience = "TestAudience")
    {
        var configMock = new Mock<IConfiguration>();

        configMock.Setup(x => x["Jwt:Key"]).Returns(key);
        configMock.Setup(x => x["Jwt:Issuer"]).Returns(issuer);
        configMock.Setup(x => x["Jwt:Audience"]).Returns(audience);

        return new TokenService(configMock.Object);
    }

    private static User CreateUser(
        string email = "user@test.com",
        string firstName = "John",
        string lastName = "Doe",
        params string[] roleNames)
    {
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = firstName,
            LastName = lastName,
            Credentials = new Credentials
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Email = email,
                PasswordHash = "hash"
            },
            UserRoles = roleNames.Select(r => new UserRole
            {
                UserId = userId,
                RoleId = Guid.NewGuid(),
                Role = new Role { Id = Guid.NewGuid(), Name = r }
            }).ToList()
        };
        return user;
    }

    // ─── Token Generation ─────────────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ShouldReturnNonEmptyString()
    {
        var service = CreateService();
        var user = CreateUser();

        var token = service.GenerateAccessToken(user);

        token.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnValidJwt()
    {
        var service = CreateService();
        var user = CreateUser();

        var token = service.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue();
    }

    // ─── Claims ───────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ShouldContainUserIdClaim()
    {
        var service = CreateService();
        var user = CreateUser();

        var token = service.GenerateAccessToken(user);
        var claims = ParseClaims(token);

        claims.Should().Contain(c =>
            c.Type == ClaimTypes.NameIdentifier &&
            c.Value == user.Id.ToString());
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainEmailClaim()
    {
        var service = CreateService();
        var user = CreateUser(email: "alice@test.com");

        var token = service.GenerateAccessToken(user);
        var claims = ParseClaims(token);

        claims.Should().Contain(c =>
            c.Type == ClaimTypes.Email &&
            c.Value == "alice@test.com");
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainFullNameClaim()
    {
        var service = CreateService();
        var user = CreateUser(firstName: "Alice", lastName: "Smith");

        var token = service.GenerateAccessToken(user);
        var claims = ParseClaims(token);

        claims.Should().Contain(c =>
            c.Type == ClaimTypes.Name &&
            c.Value == "Alice Smith");
    }

    [Fact]
    public void GenerateAccessToken_ShouldContainRoleClaims()
    {
        var service = CreateService();
        var user = CreateUser(roleNames: new[] { "Admin", "Developer" });

        var token = service.GenerateAccessToken(user);
        var claims = ParseClaims(token);

        var roleClaims = claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        roleClaims.Should().Contain("Admin");
        roleClaims.Should().Contain("Developer");
    }

    [Fact]
    public void GenerateAccessToken_WithNoRoles_ShouldNotContainRoleClaims()
    {
        var service = CreateService();
        var user = CreateUser(); // без ролей

        var token = service.GenerateAccessToken(user);
        var claims = ParseClaims(token);

        claims.Should().NotContain(c => c.Type == ClaimTypes.Role);
    }

    // ─── Expiry ───────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ShouldExpireInApproximately2Hours()
    {
        var service = CreateService();
        var user = CreateUser();

        var before = DateTime.UtcNow;
        var token = service.GenerateAccessToken(user);
        var after = DateTime.UtcNow;

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.ValidTo.Should().BeAfter(before.AddHours(1).AddMinutes(55));
        jwt.ValidTo.Should().BeBefore(after.AddHours(2).AddMinutes(5));
    }

    // ─── Issuer / Audience ────────────────────────────────────────────────────

    [Fact]
    public void GenerateAccessToken_ShouldHaveCorrectIssuerAndAudience()
    {
        var service = CreateService(issuer: "MyIssuer", audience: "MyAudience");
        var user = CreateUser();

        var token = service.GenerateAccessToken(user);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        jwt.Issuer.Should().Be("MyIssuer");
        jwt.Audiences.Should().Contain("MyAudience");
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static IEnumerable<Claim> ParseClaims(string token)
        => new JwtSecurityTokenHandler().ReadJwtToken(token).Claims;
}