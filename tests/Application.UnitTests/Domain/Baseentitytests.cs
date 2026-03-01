using Domain.Common;
using FluentAssertions;
using MediatR;

namespace Application.UnitTests.Domain;

/// <summary>
/// Тесты базовой сущности <see cref="BaseEntity"/> и механизма доменных событий.
/// </summary>
public class BaseEntityTests
{
    // ─── Конкретная реализация для тестирования абстрактного класса ──────────

    private sealed class TestEntity : BaseEntity
    {
        public void RaiseEvent(BaseEvent evt) => AddDomainEvent(evt);
    }

    private sealed class TestEvent : BaseEvent
    {
        public string Payload { get; }
        public TestEvent(string payload) => Payload = payload;
    }

    // ─── DomainEvents ─────────────────────────────────────────────────────────

    [Fact]
    public void DomainEvents_InitiallyEmpty()
    {
        var entity = new TestEntity();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_ShouldAppendEventToCollection()
    {
        var entity = new TestEntity();
        var evt = new TestEvent("payload");

        entity.RaiseEvent(evt);

        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeSameAs(evt);
    }

    [Fact]
    public void AddDomainEvent_MultipleTimes_ShouldPreserveOrder()
    {
        var entity = new TestEntity();
        var evt1 = new TestEvent("first");
        var evt2 = new TestEvent("second");
        var evt3 = new TestEvent("third");

        entity.RaiseEvent(evt1);
        entity.RaiseEvent(evt2);
        entity.RaiseEvent(evt3);

        entity.DomainEvents.Should().HaveCount(3);
        entity.DomainEvents.ElementAt(0).Should().BeSameAs(evt1);
        entity.DomainEvents.ElementAt(1).Should().BeSameAs(evt2);
        entity.DomainEvents.ElementAt(2).Should().BeSameAs(evt3);
    }

    [Fact]
    public void DomainEvents_ShouldBeReadOnly()
    {
        var entity = new TestEntity();

        // IReadOnlyCollection не должен приводиться к изменяемому типу напрямую
        entity.DomainEvents.Should().BeAssignableTo<IReadOnlyCollection<BaseEvent>>();
    }

    // ─── ClearDomainEvents ────────────────────────────────────────────────────

    [Fact]
    public void ClearDomainEvents_ShouldEmptyCollection()
    {
        var entity = new TestEntity();
        entity.RaiseEvent(new TestEvent("a"));
        entity.RaiseEvent(new TestEvent("b"));

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void ClearDomainEvents_WhenAlreadyEmpty_ShouldNotThrow()
    {
        var entity = new TestEntity();

        var act = () => entity.ClearDomainEvents();

        act.Should().NotThrow();
    }

    [Fact]
    public void ClearDomainEvents_ShouldAllowAddingNewEventsAfterwards()
    {
        var entity = new TestEntity();
        entity.RaiseEvent(new TestEvent("old"));
        entity.ClearDomainEvents();

        var newEvt = new TestEvent("new");
        entity.RaiseEvent(newEvt);

        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeSameAs(newEvt);
    }
}