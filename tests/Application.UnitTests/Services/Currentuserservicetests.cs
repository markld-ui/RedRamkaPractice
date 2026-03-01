using API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Security.Claims;

namespace Application.UnitTests.Services;

/// <summary>
/// Тесты сервиса <see cref="CurrentUserService"/>.
/// </summary>
public class CurrentUserServiceTests
{
    private static CurrentUserService CreateService(ClaimsPrincipal? principal = null)
    {
        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.Setup(x => x.User).Returns(principal ?? new ClaimsPrincipal());

        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(x => x.HttpContext).Returns(httpContextMock.Object);

        return new CurrentUserService(accessorMock.Object);
    }

    private static ClaimsPrincipal CreatePrincipal(
        Guid? userId = null,
        string? email = null,
        params string[] roles)
    {
        var claims = new List<Claim>();

        if (userId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));

        if (email is not null)
            claims.Add(new Claim(ClaimTypes.Email, email));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
    }

    // ─── UserId ───────────────────────────────────────────────────────────────

    [Fact]
    public void UserId_WhenClaimExists_ShouldReturnParsedGuid()
    {
        var userId = Guid.NewGuid();
        var service = CreateService(CreatePrincipal(userId: userId));

        service.UserId.Should().Be(userId);
    }

    [Fact]
    public void UserId_WhenClaimMissing_ShouldReturnNull()
    {
        var service = CreateService(CreatePrincipal());

        service.UserId.Should().BeNull();
    }

    [Fact]
    public void UserId_WhenClaimIsInvalidGuid_ShouldReturnNull()
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-guid") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));
        var service = CreateService(principal);

        service.UserId.Should().BeNull();
    }

    // ─── UserEmail ────────────────────────────────────────────────────────────

    [Fact]
    public void UserEmail_WhenClaimExists_ShouldReturnEmail()
    {
        var service = CreateService(CreatePrincipal(email: "user@test.com"));

        service.UserEmail.Should().Be("user@test.com");
    }

    [Fact]
    public void UserEmail_WhenClaimMissing_ShouldReturnNull()
    {
        var service = CreateService(CreatePrincipal());

        service.UserEmail.Should().BeNull();
    }

    // ─── IsAuthenticated ──────────────────────────────────────────────────────

    [Fact]
    public void IsAuthenticated_WhenUserIdPresent_ShouldReturnTrue()
    {
        var service = CreateService(CreatePrincipal(userId: Guid.NewGuid()));

        service.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void IsAuthenticated_WhenUserIdMissing_ShouldReturnFalse()
    {
        var service = CreateService(CreatePrincipal());

        service.IsAuthenticated.Should().BeFalse();
    }

    // ─── IsInRoleAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task IsInRoleAsync_WhenUserHasRole_ShouldReturnTrue()
    {
        var service = CreateService(CreatePrincipal(roles: new[] { "Admin" }));

        var result = await service.IsInRoleAsync("Admin");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsInRoleAsync_WhenUserDoesNotHaveRole_ShouldReturnFalse()
    {
        var service = CreateService(CreatePrincipal(roles: new[] { "Developer" }));

        var result = await service.IsInRoleAsync("Admin");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsInRoleAsync_WhenNoHttpContext_ShouldReturnFalse()
    {
        var accessorMock = new Mock<IHttpContextAccessor>();
        accessorMock.Setup(x => x.HttpContext).Returns((HttpContext?)null);

        var service = new CurrentUserService(accessorMock.Object);

        var result = await service.IsInRoleAsync("Admin");

        result.Should().BeFalse();
    }
}