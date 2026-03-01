using Domain.Projects;
using FluentAssertions;

namespace Application.UnitTests.Domain;

/// <summary>
/// Тесты доменной сущности <see cref="ProjectSpecification"/>.
/// </summary>
public class ProjectSpecificationTests
{
    private static ProjectSpecification CreateSpec(int version = 1, string content = "content")
        => new(version, content, Guid.NewGuid());

    // ─── Construction ─────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_ShouldSetIsApprovedFalse()
    {
        var spec = CreateSpec();

        spec.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAt()
    {
        var before = DateTime.UtcNow;
        var spec = CreateSpec();

        spec.CreatedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Constructor_ShouldSetVersion()
    {
        var spec = CreateSpec(version: 3);

        spec.Version.Should().Be(3);
    }

    // ─── Approve ──────────────────────────────────────────────────────────────

    [Fact]
    public void Approve_ShouldSetIsApprovedTrue()
    {
        var spec = CreateSpec();

        spec.Approve();

        spec.IsApproved.Should().BeTrue();
    }

    [Fact]
    public void Approve_ShouldSetApprovedAt()
    {
        var spec = CreateSpec();
        var before = DateTime.UtcNow;

        spec.Approve();

        spec.ApprovedAt.Should().BeAfter(before);
    }

    [Fact]
    public void Approve_WhenAlreadyApproved_ShouldBeIdempotent()
    {
        var spec = CreateSpec();
        spec.Approve();
        var firstApprovedAt = spec.ApprovedAt;

        spec.Approve(); // повторный вызов — должен игнорироваться

        spec.ApprovedAt.Should().Be(firstApprovedAt);
    }

    // ─── Revoke ───────────────────────────────────────────────────────────────

    [Fact]
    public void Revoke_ShouldSetIsApprovedFalse()
    {
        var spec = CreateSpec();
        spec.Approve();

        spec.Revoke();

        spec.IsApproved.Should().BeFalse();
    }

    [Fact]
    public void Revoke_WhenNotApproved_ShouldBeIdempotent()
    {
        var spec = CreateSpec();

        var act = () => spec.Revoke();

        act.Should().NotThrow();
        spec.IsApproved.Should().BeFalse();
    }
}