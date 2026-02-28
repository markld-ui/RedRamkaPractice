using MediatR;

namespace Domain.Common;

/// <summary>
/// Базовый класс для всех доменных событий.
/// </summary>
/// <remarks>
/// Реализует интерфейс <see cref="INotification"/> для публикации событий
/// через механизм MediatR. Все производные классы автоматически фиксируют
/// время возникновения события в момент создания экземпляра.
/// </remarks>
public abstract class BaseEvent : INotification
{
    /// <summary>
    /// Возвращает дату и время возникновения события в формате UTC.
    /// </summary>
    /// <value>
    /// Значение <see cref="DateTime"/> в формате UTC, установленное в момент создания события.
    /// </value>
    public DateTime OccurredOn { get; protected set; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="BaseEvent"/>,
    /// фиксируя текущее время в качестве момента возникновения события.
    /// </summary>
    protected BaseEvent()
    {
        OccurredOn = DateTime.UtcNow;
    }
}