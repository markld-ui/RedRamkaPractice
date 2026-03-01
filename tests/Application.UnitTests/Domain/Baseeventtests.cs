using Domain.Common;
using Domain.Events;
using Domain.Projects;
using FluentAssertions;

namespace Application.UnitTests.Domain;

/// <summary>
/// Тесты базового класса доменных событий <see cref="BaseEvent"/>.
/// </summary>
public class BaseEventTests
{
    private sealed class ConcreteEvent : BaseEvent { }

    [Fact]
    public void Constructor_ShouldSetOccurredOnToUtcNow()
    {
        var before = DateTime.UtcNow;
        var evt = new ConcreteEvent();
        var after = DateTime.UtcNow;

        evt.OccurredOn.Should().BeOnOrAfter(before);
        evt.OccurredOn.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void OccurredOn_ShouldBeUtcKind()
    {
        var evt = new ConcreteEvent();

        evt.OccurredOn.Kind.Should().Be(DateTimeKind.Utc);
    }

    [Fact]
    public void TwoEvents_CreatedSequentially_ShouldHaveNonDecreasingOccurredOn()
    {
        var evt1 = new ConcreteEvent();
        var evt2 = new ConcreteEvent();

        evt2.OccurredOn.Should().BeOnOrAfter(evt1.OccurredOn);
    }
}

/// <summary>
/// Тесты доменного события <see cref="ProjectCreatedEvent"/>.
/// </summary>
public class ProjectCreatedEventTests
{
    [Fact]
    public void Constructor_WithValidProject_ShouldStoreProject()
    {
        var project = new Project("Test", "Desc");

        var evt = new ProjectCreatedEvent(project);

        evt.Project.Should().BeSameAs(project);
    }

    [Fact]
    public void Constructor_WithNullProject_ShouldThrow()
    {
        var act = () => new ProjectCreatedEvent(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("project");
    }

    [Fact]
    public void Constructor_ShouldSetOccurredOn()
    {
        var before = DateTime.UtcNow;
        var project = new Project("Test", "Desc");

        var evt = new ProjectCreatedEvent(project);

        evt.OccurredOn.Should().BeOnOrAfter(before);
    }
}