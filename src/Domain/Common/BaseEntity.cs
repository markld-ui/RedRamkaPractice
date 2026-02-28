using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Common;

/// <summary>
/// Базовый класс для всех доменных сущностей, поддерживающий механизм доменных событий.
/// </summary>
public abstract class BaseEntity
{
    [NotMapped]
    private readonly List<BaseEvent> _domainEvents = new();

    /// <summary>
    /// Возвращает коллекцию доменных событий, накопленных сущностью.
    /// </summary>
    /// <value>
    /// Доступная только для чтения коллекция объектов <see cref="BaseEvent"/>.
    /// </value>
    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Добавляет доменное событие в коллекцию событий сущности.
    /// </summary>
    /// <param name="domainEvent">Доменное событие, которое необходимо зарегистрировать.</param>
    protected void AddDomainEvent(BaseEvent domainEvent)
        => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Очищает коллекцию доменных событий сущности.
    /// </summary>
    /// <remarks>
    /// Следует вызывать после успешной публикации всех накопленных событий.
    /// </remarks>
    public void ClearDomainEvents()
        => _domainEvents.Clear();
}